# This helper makes sure we have the LLVM tool we require present on disk.
# If it's not available then we download it from GitHub.
 
# REQUIRES "io.ps1" to be included

# ------------------------------------------------------------------------------------------------
# Default install locations of LLVM. If present there, we automatically use it

Set-Variable -name kLLVMInstallLocations    -value @("${Env:ProgramW6432}\LLVM\bin"
                                                    ,"${Env:ProgramFiles(x86)}\LLVM\bin"
                                                    )                   -option Constant

Function Test-InternetConnectivity
{  
  $resp = Get-WmiObject -Class Win32_PingStatus -Filter 'Address="github.com" and Timeout=100' | Select-Object ResponseTime
  [bool] $hasInternetConnectivity = ($resp.ResponseTime -and $resp.ResponseTime -gt 0)
  return $hasInternetConnectivity
}

#Url to assets (clang++ and clang-tidy) from previous release made by Clang Power Tools on github 
Set-Variable -name kCptGithubLlvm -value "https://github.com/Caphyon/clang-power-tools/releases/download/v8.4.0" `
                                  -option Constant
Set-Variable -name kCptGithubLlvmVersion -value "14.0.0 (LLVM 14.0.0)" -Option Constant

Function Ensure-LLVMTool-IsPresent($clangToolWeNeed) {
  [string] $ret = ""

  # see if we can reach the tool through PATH
  if (Exists-Command $clangToolWeNeed )
  {
    [System.IO.FileInfo] $toolPath = (Get-Command $clangToolWeNeed).Source
    return $toolPath.Directory.FullName
  }
  
  # search in predefined locations
  foreach ($locationLLVM in $kLLVMInstallLocations)
  {
    if (Test-Path -LiteralPath "$locationLLVM\$clangToolWeNeed")
    {
      return $locationLLVM
    }
  }

  # download read-to-use binary from github
  if (Test-InternetConnectivity)
  {
    [string] $llvmLiteDirParent = "${env:APPDATA}\ClangPowerTools"
    [string] $llvmLiteDir       = "$llvmLiteDirParent\LLVM_Lite"

    [string] $llvmLiteToolPath = "$llvmLiteDir\$clangToolWeNeed"
    if (Test-Path $llvmLiteToolPath)
    {
      $versionPresent = (Get-Item $llvmLiteToolPath).VersionInfo.ProductVersion
      if ($versionPresent -eq $kCptGithubLlvmVersion)
      {
        # we already have downloaded the latest standalone tool, reuse it
        $ret = $llvmLiteDir
      }
    }

    if ([string]::IsNullOrEmpty($ret))
    {
      if (!(Test-Path $llvmLiteDirParent))
      {
        New-Item -Path $llvmLiteDirParent -ItemType Directory | Out-Null
      }
      if (!(Test-Path $llvmLiteDir))
      {
        New-Item -Path $llvmLiteDir -ItemType Directory | Out-Null
      }

      # the displayed progress slows downloads considerably, so disable it
      $prevPreference = $ProgressPreference
      $ProgressPreference = 'SilentlyContinue'
      [string] $clangCompilerWebPath = "$kCptGithubLlvm/$clangToolWeNeed"
      
      if (Test-Path  $llvmLiteToolPath)
      {
        # we have an older version downloaded, remove it first
        Remove-Item $llvmLiteToolPath -Force
      }

      Write-Verbose "Downloading $clangToolWeNeed $kCptGithubLlvmVersion ..."
      # grab ready-to-use LLVM binaries from Github
      Invoke-WebRequest -Uri $clangCompilerWebPath -OutFile $llvmLiteToolPath
      $ProgressPreference = $prevPreference

      $ret = $llvmLiteDir
    }
  }
  return $ret
}
