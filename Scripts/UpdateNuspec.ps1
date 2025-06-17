param (
  [Parameter(Mandatory = $true)]
  [string] $CsprojPath,
  [Parameter(Mandatory = $true)]
  [string] $NuspecPath,
  [string[]] $AdditionalFiles = @(),
  [string[]] $ExternalCsprojDependencies = @()
)

if (!(Test-Path -Path $csprojPath)) {
  Write-Error "File '$($csprojPath)' not found."
  exit;
}

if (!(Test-Path -Path $nuspecPath)) {
  Write-Error "File '$($nuspecPath)' not found."
  exit;
}

function Get-RelativePath {
  param(
    [string]$RelativeTo,
    [string]$Path
  )

  $uri = New-Object System.Uri($RelativeTo)
  $rel = [System.Uri]::UnescapeDataString($uri.MakeRelativeUri((New-Object System.Uri([IO.Path]::Combine((Get-Location), $Path)))).ToString()).Replace([System.IO.Path]::AltDirectorySeparatorChar, [System.IO.Path]::DirectorySeparatorChar)
  if (!($rel -match "[\\/]")) {
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
  $csproj = [xml](Get-Content $CsprojPath)
  $packageReferences = $csproj.Project.ItemGroup.PackageReference | Where-Object { $_ -ne $null }
  $projectReferences = $csproj.Project.ItemGroup.ProjectReference | Where-Object { $_ -ne $null }
  $targetFrameworks = GetTargetFrameworks -Xml $csproj

  $output = @{}
  foreach ($targetFramework in $targetFrameworks) {
    $output[$targetFramework] = @{}
    foreach ($packageReference in $packageReferences) {
      if ($output[$targetFramework].ContainsKey($packageReference.Include)) {
        continue
      }
      $output[$targetFramework][$packageReference.Include] = @{
        id      = $packageReference.Include
        version = $packageReference.Version
        include = $projectReference.IncludeAssets
      }
    }
  }

  # Gather dependencies from referenced projects
  foreach ($projectReference in $projectReferences) {
    if ($projectReference.PrivateAssets -ne "All") {
      continue;
    }
    
    if ([string]::IsNullOrWhitespace($projectReference.IncludeAssets)) {
      continue;
    }

    $dependencyProjectCsproj = [xml](Get-Content "$([IO.Path]::Combine($RootDir, $projectReference.Include))")
    $dependencyProjectTargetFrameworks = GetTargetFrameworks -Xml $dependencyProjectCsproj
    $dependencyPackageReferences = $dependencyProjectCsproj.Project.ItemGroup.PackageReference | Where-Object { $_ -ne $null }
    foreach ($targetFramework in (@($targetFrameworks) + @($dependencyProjectTargetFrameworks)) | Select-Object -Unique) {
      if (!$output.ContainsKey($targetFramework)) {
        $output[$targetFramework] = @{}
      }

      foreach ($dependencyPackageReference in $dependencyPackageReferences) {
        if ($output[$targetFramework].ContainsKey($dependencyPackageReference.Include)) {
          continue
        }
        
        $output[$targetFramework][$dependencyPackageReference.Include] = @{
          id      = $dependencyPackageReference.Include
          version = $dependencyPackageReference.Version
          include = $projectReference.IncludeAssets
        }
      }
    }

    $dependencyProjectReferences = GatherDependencies -CsprojPath "$([IO.Path]::Combine($RootDir, $projectReference.Include))" -RootDir $RootDir
    foreach ($targetFramework in $dependencyProjectReferences.Keys | Where-Object { $_ -ne $null }) {
      if (!$output.ContainsKey($targetFramework)) {
        $output[$targetFramework] = @{}
      }
      
      foreach ($packageId in $dependencyProjectReferences[$targetFramework].Keys) {
        if (($output[$targetFramework].ContainsKey($packageId))) {
          continue
        }

        $output[$targetFramework][$packageId] = $dependencyProjectReferences[$targetFramework][$packageId]
      }
    }
  }

  return $output
}


$csprojPath = (Resolve-Path $csprojPath).Path
$nuspecPath = (Resolve-Path $nuspecPath).Path
$rootCsprojDirectory = (Split-Path $CsprojPath -Parent)
$rootCsproj = [xml](Get-Content $csprojPath)
$nuspecXml = [xml](Get-Content $nuspecPath)
$nuspecXmlNamespace = $nuspecXml.package.xmlns
$rootTargetFrameworks = GetTargetFrameworks -Xml $rootCsproj
$rootPackAsATool = $rootCsproj.project.PropertyGroup.PackAsTool -ieq "true"

# metadata.dependencies
foreach ($node in $nuspecXml.package.metadata.SelectNodes("//*[local-name() = 'dependencies']")) {
  $node.ParentNode.RemoveChild($node) | Out-Null
}

$nuspecDependencies = GatherDependencies -CsprojPath $csprojPath -RootDir $rootCsprojDirectory

if ($null -ne $ExternalCsprojDependencies) {
  foreach ($path in $ExternalCsprojDependencies) {
    $externalDependencies = GatherDependencies -CsprojPath $path -RootDir $rootCsprojDirectory
    foreach ($targetFramework in $externalDependencies.Keys) {
      if (!$nuspecDependencies.ContainsKey($targetFramework)) {
        $nuspecDependencies[$targetFramework] = @{}
      }
      
      foreach ($packageId in $externalDependencies[$targetFramework].Keys) {
        $externalDependency = $externalDependencies[$targetFramework][$packageId]
        foreach ($rootTargetFramework in $rootTargetFrameworks) {
          if (($nuspecDependencies[$targetFramework].ContainsKey($packageId))) {
            $nuspecDependencies[$targetFramework].Remove($packageId)
          }

          $nuspecDependencies[$rootTargetFramework][$packageId] =  @{
            id      = $externalDependency.id
            version = $externalDependency.Version
          }
        }
      }
    }
  }
}

$dependenciesNode = $nuspecXml.CreateElement("dependencies", $nuspecXmlNamespace)
foreach ($targetFramework in $nuspecDependencies.Keys) {
  $groupNode = $nuspecXml.CreateElement("group", $nuspecXmlNamespace)
  $targetFrameworkAttr = $nuspecXml.CreateAttribute('targetFramework')
  $targetFrameworkAttr.Value = $targetFramework
  $groupNode.Attributes.Append($targetFrameworkAttr) | Out-Null
  foreach ($packageId in $nuspecDependencies[$targetFramework].Keys | Sort-Object) {
    $item = $nuspecDependencies[$targetFramework][$packageId]
    $dependencyNode = $nuspecXml.CreateElement("dependency", $nuspecXmlNamespace)

    $idAttr = $nuspecXml.CreateAttribute('id')
    $idAttr.Value = $item.id
    $dependencyNode.Attributes.Append($idAttr) | Out-Null

    $versionAttr = $nuspecXml.CreateAttribute('version')
    $versionAttr.Value = $item.version
    $dependencyNode.Attributes.Append($versionAttr) | Out-Null

    if (![string]::IsNullOrWhitespace($item.include)) {
      $includeAttr = $nuspecXml.CreateAttribute('include')
      $includeAttr.Value = $item.include
      $dependencyNode.Attributes.Append($includeAttr) | Out-Null
    }

    $excludeAttr = $nuspecXml.CreateAttribute('exclude')
    $excludeAttr.Value = "Build,Analyzers"
    $dependencyNode.Attributes.Append($excludeAttr) | Out-Null

    $groupNode.AppendChild($dependencyNode) | Out-Null
  }

  if ($groupNode.ChildNodes.Count -gt 0) {
    $dependenciesNode.AppendChild($groupNode) | Out-Null
  }
}

$nuspecXml.package.metadata.AppendChild($dependenciesNode) | Out-Null

# metadata.frameworkReferences
foreach ($node in $nuspecXml.package.metadata.SelectNodes("//*[local-name() = 'frameworkReferences']")) {
  $node.ParentNode.RemoveChild($node) | Out-Null
}

$frameworkReferences = $rootCsproj.Project.ItemGroup.FrameworkReference | Where-Object { $_ -ne $null }
$frameworkReferencesNode = $null
if ($frameworkReferences.Count -ne 0) {
  $frameworkReferencesNode = $nuspecXml.CreateElement("frameworkReferences", $nuspecXmlNamespace)
}

if ($null -ne $frameworkReferencesNode) {
  foreach ($targetFramework in $rootTargetFrameworks) {
    $groupNode = $nuspecXml.CreateElement("group", $nuspecXmlNamespace)
    $targetFrameworkAttr = $nuspecXml.CreateAttribute('targetFramework')
    $targetFrameworkAttr.Value = $targetFramework
    $groupNode.Attributes.Append($targetFrameworkAttr) | Out-Null
  
    foreach ($frameworkReference in $frameworkReferences) {
      $frameworkReferenceNode = $nuspecXml.CreateElement("frameworkReference", $nuspecXmlNamespace)
      $nameAttr = $nuspecXml.CreateAttribute('name')
      $nameAttr.Value = $frameworkReference.Include
      $frameworkReferenceNode.Attributes.Append($nameAttr) | Out-Null
      $groupNode.AppendChild($frameworkReferenceNode) | Out-Null
    }

    $frameworkReferencesNode.AppendChild($groupNode) | Out-Null
  }
  
  if ($frameworkReferencesNode.ChildNodes.Count -gt 0) {
    $nuspecXml.package.metadata.AppendChild($frameworkReferencesNode) | Out-Null
  }
}

# Files
foreach ($node in $nuspecXml.package.SelectNodes("//*[local-name() = 'files']")) {
  $node.ParentNode.RemoveChild($node) | Out-Null
}

$filesNode = $nuspecXml.CreateElement("files", $nuspecXmlNamespace)
$allowedFileExtensions = @(".dll", ".pdb", ".exe", ".json", ".xml") 
foreach ($targetFramework in $rootTargetFrameworks) {
  foreach ($file in (Get-ChildItem -Path "$([IO.Path]::Combine($rootCsprojDirectory, "bin", "Release", $targetFramework))")) {
    if (-not($allowedFileExtensions -Contains $file.Extension)) {
      continue
    }
      
    $fileNode = $nuspecXml.CreateElement("file", $nuspecXmlNamespace)

    $srcAttr = $nuspecXml.CreateAttribute('src')
    $srcAttr.Value = "$([IO.Path]::Combine("bin", "Release", $targetFramework, $file.Name))"
    $fileNode.Attributes.Append($srcAttr) | Out-Null

    $targetAttr = $nuspecXml.CreateAttribute('target')
    if ($rootPackAsATool) {
      $targetAttr.Value = "$([IO.Path]::Combine("tools", $targetFramework, "any", $file.Name))"
    } else {
      $targetAttr.Value = "$([IO.Path]::Combine("lib", $targetFramework, $file.Name))"
    }

    $fileNode.Attributes.Append($targetAttr) | Out-Null
    $filesNode.AppendChild($fileNode) | Out-Null
  }
}

# $path can be path or docker like foo/source.ext:boo/target.ext
foreach ($path in $AdditionalFiles) {
  $source = $path -split ":"
  $target = [System.IO.Path]::DirectorySeparatorChar.ToString()
  if ($path -match ":" -and -not([string]::IsNullOrWhitespace($source[1]))) {
    $target = $source[1]
    $source = $source[0]
  }

  $fileNode = $nuspecXml.CreateElement("file", $nuspecXmlNamespace)
  $srcAttr = $nuspecXml.CreateAttribute('src')
  $srcAttr.Value = Get-RelativePath -RelativeTo $CsprojPath -Path $source
  $fileNode.Attributes.Append($srcAttr) | Out-Null

  $targetAttr = $nuspecXml.CreateAttribute('target')
  $targetAttr.Value = $target
  $fileNode.Attributes.Append($targetAttr) | Out-Null

  $filesNode.AppendChild($fileNode) | Out-Null
}

$nuspecXml.package.AppendChild($filesNode) | Out-Null


# Increment version
$currentTargetFrameworks = @(([xml](Get-Content $NuspecPath)).package.metadata.dependencies.group.targetFramework)
$hasTargetFrameworksChanged = [Bool](Compare-Object -ReferenceObject $rootTargetFrameworks -DifferenceObject $currentTargetFrameworks)

$currentVersion = $nuspecXml.package.metadata.version
$versionSplitted = $currentVersion -split '\.'
if ($hasTargetFrameworksChanged) {
  $versionSplitted[1] = ([int]$versionSplitted[1]) + 1
  $versionSplitted[2] = 0
} else {
  $versionSplitted[2] = ([int]$versionSplitted[2]) + 1
}
$nuspecXml.package.metadata.version = $versionSplitted -join '.'

# Update commit hash
$commitAttr = $nuspecXml.CreateAttribute('commit')
$commitAttr.Value = $(git rev-parse HEAD)
$nuspecXml.package.metadata.repository.Attributes.Append($commitAttr) | Out-Null

$nuspecXml.Save($NuspecPath);
