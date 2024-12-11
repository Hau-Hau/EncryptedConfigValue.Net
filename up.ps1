$TargetDotnetVersions = "net9.0" -split ","
        $SelectedProject = "EncryptedConfigValue.Module"
        $csProjPath = "$([IO.Path]::Combine(".", "$($SelectedProject)", "$($SelectedProject).csproj"))"
        $csprojXml = [xml](Get-Content $csProjPath)

        $projectTargetFrameworksStr = $csprojXml.Project.PropertyGroup.TargetFrameworks
        if (![string]::IsNullOrWhitespace($projectTargetFrameworksStr)) {
          $projectTargetFrameworks = @($projectTargetFrameworksStr -split ';' -match '\S')
          $hasDifferences = [Bool](Compare-Object -ReferenceObject $projectTargetFrameworks -DifferenceObject $TargetDotnetVersions)
          "result=$("$($hasDifferences)".ToLowerInvariant())" >> $env:GITHUB_OUTPUT
          exit 0  
        }

        $projectTargetFramework = $csprojXml.Project.PropertyGroup.TargetFramework
        if ([string]::IsNullOrWhitespace($projectTargetFramework)) {
          Write-Host "No TargetFrameworks or TargetFramework found in $($path)"
          exit 1
        }

        $numericVersions = $TargetDotnetVersions -replace "^net" | ForEach-Object { [decimal]$_ }
        $checkNumeric = $projectTargetFramework -replace "^net" | ForEach-Object { [decimal]$_ }
        
        $highestVersion = ($numericVersions | Measure-Object -Maximum).Maximum
        "result=$("$($checkNumeric -ne $highestVersion)".ToLowerInvariant())" >> $env:GITHUB_OUTPUT
