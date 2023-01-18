# This helper makes sure we have the LLVM tool we require present on disk.
# If it's not available then we download it from GitHub.
 
# REQUIRES "io.ps1" to be included

# ------------------------------------------------------------------------------------------------
# Default install locations of LLVM. If present there, we automatically use it

Set-Variable -name kLLVMInstallLocations    -value @("${Env:ProgramW6432}\LLVM\bin"
                                                    ,"${Env:ProgramFiles(x86)}\LLVM\bin"
                                                    )                   -option Constant
#Url to assets (clang++ and clang-tidy) from previous release made by Clang Power Tools on github 
Set-Variable -name kCptGithubLlvm -value "https://github.com/Caphyon/clang-power-tools/releases/download/v2023.1.1" `
                                  -option Constant
Set-Variable -name kCptGithubLlvmVersion -value "15.0.7 (LLVM 15.0.7)" -Option Constant

# Clang Constants

Set-Variable -name kCss            -value "clang-doc-default-stylesheet.css"       -option Constant
Set-Variable -name kClangDoc       -value "clang-doc.exe"                          -option Constant
Set-Variable -name kIndex          -value "index.js"                               -option Constant

Function Move-Tool-To-LlvmBin([Parameter(Mandatory = $true)][string] $clangToolWeNeed,
                              [Parameter(Mandatory = $true)][string] $llvmLiteBinDir)
{

  $llvmLiteDir = (get-item $llvmLiteBinDir).Parent.FullName

  if(Test-Path "$llvmLiteDir\$clangToolWeNeed")
  {
    Move-Item -Path "$llvmLiteDir\$clangToolWeNeed" -Destination "$llvmLiteBinDir\$clangToolWeNeed"
  }

}

Function Ensure-LLVMTool-IsPresent([Parameter(Mandatory = $true)][string] $clangToolWeNeed) {
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

  [string] $llvmLiteDirParent = "${env:APPDATA}\ClangPowerTools"
  [string] $llvmLiteDir       = "$llvmLiteDirParent\LLVM_Lite\Bin"

  [string] $llvmLiteToolPath = "$llvmLiteDir\$clangToolWeNeed"
  if (Test-Path $llvmLiteToolPath)
  {
    $versionPresent = (Get-Item $llvmLiteToolPath).VersionInfo.ProductVersion
    if ($versionPresent -eq $kCptGithubLlvmVersion)
    {
      # we already have downloaded the latest standalone tool, reuse it
      return $llvmLiteDir
    }
  }

  if (Test-InternetConnectivity)
  {
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
      
      # check if tool already exists Llvm_lite folder, to move it in Llvm_lite/bin
      Move-Tool-To-LlvmBin $clangToolWeNeed $llvmLiteDir

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
      # download css file if needed tool is clang-doc.exe
      if($clangToolWeNeed -eq $kClangDoc)
      {
        [string] $parentDirLite = (get-item $llvmLiteDir ).Parent.FullName
        [string] $llvmLiteCssFolderPath = "$parentDirLite\share\clang"
        if (!(Test-Path $llvmLiteCssFolderPath))
        {
          New-Item $llvmLiteCssFolderPath -ItemType Directory | Out-Null
        }
        Invoke-WebRequest -Uri "$kCptGithubLlvm/$kCss" -OutFile "$llvmLiteCssFolderPath\$kCss"
        Invoke-WebRequest -Uri "$kCptGithubLlvm/$kIndex" -OutFile "$llvmLiteCssFolderPath\$kIndex"
      } 
      $ProgressPreference = $prevPreference

      $ret = $llvmLiteDir
    }
  }
  return $ret
}
