param (
  [Parameter(Mandatory = $true)]
  [string] $CsprojPath,
  [Parameter(Mandatory = $true)]
  [string] $NuspecPath,
  [string] $ReadmePath
)

if (!(Test-Path -Path $csprojPath)) {
  Write-Error "File '$($csprojPath)' not found."
}

if (!(Test-Path -Path $nuspecPath)) {
  Write-Error "File '$($nuspecPath)' not found."
  exit;
}

function GetRelativePath {
  param(
    [string]$RelativeTo,
    [string]$Path
  )

  $uri = New-Object System.Uri($RelativeTo)
  $rel = [System.Uri]::UnescapeDataString($uri.MakeRelativeUri((New-Object System.Uri($Path))).ToString()).Replace([System.IO.Path]::AltDirectorySeparatorChar, [System.IO.Path]::DirectorySeparatorChar)
  if (!$rel.Contains([System.IO.Path]::DirectorySeparatorChar.ToString())) {
    $rel = ".$([System.IO.Path]::DirectorySeparatorChar)$($rel)"
  }

  return $rel
}

function GetTargetFrameworks {
  param ($Xml)
  $targetFrameworksStr = $Xml.Project.PropertyGroup.TargetFrameworks
  if ([string]::IsNullOrWhitespace($targetFrameworksStr)) {
    $targetFrameworksStr = $Xml.Project.PropertyGroup.TargetFramework
  }
  
  if ([string]::IsNullOrWhitespace($targetFrameworksStr)) {
    return @()
  }
  
  return @($targetFrameworksStr -split ';' -match '\S') # remove empty entries
}

function GatherDependencies {
  param ($CsprojPath, $RootDir)
  $output = @{}
  $csproj = [xml](Get-Content $CsprojPath)
  $projectReferences = $csproj.Project.ItemGroup.ProjectReference
  $targetFrameworks = GetTargetFrameworks -Xml $csproj
  foreach ($projectReference in $projectReferences) {
    if ($projectReference.PrivateAssets -ne "All") {
      continue;
    }
    
    if ([string]::IsNullOrWhitespace($projectReference.IncludeAssets)) {
      continue;
    }

    $dependencyPackageReferences = (Select-Xml -Path "$([IO.Path]::Combine($RootDir, $projectReference.Include))" -XPath '/Project/ItemGroup/PackageReference').Node
    foreach ($targetFramework in $targetFrameworks) {
      $output[$targetFramework] = @()
      foreach ($dependencyPackageReference in $dependencyPackageReferences) {
        $output[$targetFramework] += @{
          id      = $dependencyPackageReference.Include
          version = $dependencyPackageReference.Version
          include = $projectReference.IncludeAssets
        }
      }
    }

    $dependencyProjectReferences = GatherDependencies -CsprojPath $projectReference.Include -RootDir $RootDir
    foreach ($key in $dependencyProjectReferences.Keys) {
      if (!$output.ContainsKey($key)) {
        $output[$key] = @()
      }
        
      foreach ($value in $dependencyPackageReferences[$key]) {
        $output[$key] += $value
      }
    }
  }

  return $output
}


$csprojPath = (Resolve-Path $csprojPath).Path
$nuspecPath = (Resolve-Path $nuspecPath).Path
$rootCsprojDirectory = (Split-Path $CsprojPath -Parent)

$nuspecDependencies = GatherDependencies -CsprojPath $csprojPath -RootDir $rootCsprojDirectory
$nuspecXml = [xml](Get-Content $nuspecPath)
$xmlNamespace = $nuspecXml.package.xmlns

if ($null -ne $nuspecXml.package.metadata.dependencies) {
  foreach ($parameter in $nuspecXml.package.metadata.dependencies) {
    $parameter.ParentNode.RemoveChild($parameter) | out-null
  }
}

$dependenciesNode = $nuspecXml.CreateElement("dependencies", $xmlNamespace)
foreach ($key in $nuspecDependencies.Keys) {
  $groupNode = $nuspecXml.CreateElement("group", $xmlNamespace)
  $targetFrameworkAttr = $nuspecXml.CreateAttribute('targetFramework')
  $targetFrameworkAttr.Value = $key
  $groupNode.Attributes.Append($targetFrameworkAttr) | out-null
  foreach ($item in $nuspecDependencies[$key]) {
    $dependencyNode = $nuspecXml.CreateElement("dependency", $xmlNamespace)
    
    $idAttr = $nuspecXml.CreateAttribute('id')
    $idAttr.Value = $item.id
    $dependencyNode.Attributes.Append($idAttr) | out-null
    
    $versionAttr = $nuspecXml.CreateAttribute('version')
    $versionAttr.Value = $item.version
    $dependencyNode.Attributes.Append($versionAttr) | out-null
    
    $includeAttr = $nuspecXml.CreateAttribute('include')
    $includeAttr.Value = $item.include
    $dependencyNode.Attributes.Append($includeAttr) | out-null

    $groupNode.AppendChild($dependencyNode) | out-null
  }
  $dependenciesNode.AppendChild($groupNode) | out-null
}
    

foreach ($node in $nuspecXml.package.files) {
  if ($node.Name -ne "files") {
    continue
  }

  $node.ParentNode.RemoveChild($node) | out-null
}
$nuspecXml.package.metadata.AppendChild($dependenciesNode) | out-null

$filesNode = $nuspecXml.CreateElement("files", $xmlNamespace)
$targetFrameworks = GetTargetFrameworks -Xml ([xml](Get-Content $csprojPath))

$appendFiles = $false
foreach ($targetFramework in $targetFrameworks) {
  foreach ($file in (Get-ChildItem -Path "$([IO.Path]::Combine($rootCsprojDirectory, "bin", "Release", $targetFramework))")) {
    if (($file.Extension -ne ".dll") -and ($file.Extension -ne ".pdb")) {
      continue
    }
      
    $fileNode = $nuspecXml.CreateElement("file", $xmlNamespace)

    $srcAttr = $nuspecXml.CreateAttribute('src')
    $srcAttr.Value = "$([IO.Path]::Combine("bin", "Release", $targetFramework, $file.Name))"
    $fileNode.Attributes.Append($srcAttr) | out-null

    $targetAttr = $nuspecXml.CreateAttribute('target')
    $targetAttr.Value = "$([IO.Path]::Combine("lib", $targetFramework, $file.Name))"
    $fileNode.Attributes.Append($targetAttr) | out-null

    $filesNode.AppendChild($fileNode) | out-null

    if ($appendFiles -eq $false) {
      $appendFiles = $true
    }
  }
}

if ($appendFiles -eq $true) { 
  if ($null -ne $ReadmePath) {
    $fileNode = $nuspecXml.CreateElement("file", $xmlNamespace)
    $srcAttr = $nuspecXml.CreateAttribute('src')
    $srcAttr.Value = GetRelativePath -RelativeTo $CsprojPath -Path $ReadmePath
    $fileNode.Attributes.Append($srcAttr) | out-null

    $targetAttr = $nuspecXml.CreateAttribute('target')
    $targetAttr.Value = "\"
    $fileNode.Attributes.Append($targetAttr) | out-null

    $filesNode.AppendChild($fileNode) | out-null
  }
 
  $nuspecXml.package.AppendChild($filesNode) | out-null
}
else {
  Write-Error "No files found!"
}

$nuspecXml.Save($NuspecPath);
