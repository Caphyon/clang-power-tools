<#
.SYNOPSIS
  Download and update clang-format.exe for a given LLVM version.

.DESCRIPTION
  - Downloads the official LLVM Windows installer from GitHub releases:
      https://github.com/llvm/llvm-project/releases/download/llvmorg-<VERSION>/LLVM-<VERSION>-win64.exe
  - Runs the installer silently into a temporary folder.
  - Locates clang-format.exe in the installed tree.
  - Replaces:
      ClangPowerTools/ClangPowerToolsShared/Executables/clang-format.exe

.PARAMETER LlvmVersion
  LLVM version, e.g. "20.1.0" or "21.1.6".

.PARAMETER RepoRoot
  (Optional) Path to the repository root.
  Defaults to one level above the script location.

.EXAMPLE
  .\update-clang-format.ps1 -LlvmVersion 21.1.6
#>

[CmdletBinding()]
param (
    [Parameter(Mandatory = $true)]
    [string] $LlvmVersion,

    [Parameter(Mandatory = $false)]
    [string] $RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

# -------- Config --------

# GitHub tag prefix
$GithubTagPrefix = "llvmorg"

# Asset file name pattern
# Example: LLVM-21.1.6-win64.exe
$AssetFileName = "LLVM-$LlvmVersion-win64.exe"

# Target clang-format path (relative to repo root)
$TargetRelativePath = "ClangPowerTools\ClangPowerToolsShared\Executables\clang-format.exe"

# -------- Derived paths & URLs --------

$tag = "$GithubTagPrefix-$LlvmVersion"
$baseDownloadUrl = "https://github.com/llvm/llvm-project/releases/download/$tag"
$downloadUrl = "$baseDownloadUrl/$AssetFileName"

$targetPath = Join-Path $RepoRoot $TargetRelativePath

# Temp folder for download & installation
$tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) "cpt-update-clang-format-$($LlvmVersion.Replace('.','-'))"
$null = New-Item -ItemType Directory -Path $tempRoot -Force

$downloadPath = Join-Path $tempRoot $AssetFileName
$installDir   = Join-Path $tempRoot "install"

Write-Host "LLVM version      : $LlvmVersion"
Write-Host "GitHub tag        : $tag"
Write-Host "Download URL      : $downloadUrl"
Write-Host "Repository root   : $RepoRoot"
Write-Host "Target exe path   : $targetPath"
Write-Host ""

try {
    # -------- Download installer --------
    Write-Host "Downloading LLVM installer..."
    Invoke-WebRequest -Uri $downloadUrl -OutFile $downloadPath -UseBasicParsing

    if (-not (Test-Path $downloadPath)) {
        throw "Download failed: file not found at $downloadPath"
    }

    # -------- Run installer silently into temp folder --------
    Write-Host "Running installer silently into: $installDir"
    $null = New-Item -ItemType Directory -Path $installDir -Force

    # LLVM Windows installers are NSIS-based, which typically support:
    #   /S              = silent
    #   /D=<path>       = install directory (no quotes, path must be last arg)
    #
    # We pass arguments as a single string where /D= is last.
    $installArgs = "/S /D=$installDir"

    $process = Start-Process -FilePath $downloadPath `
                             -ArgumentList $installArgs `
                             -Wait `
                             -PassThru

    if ($process.ExitCode -ne 0) {
        throw "Installer exited with code $($process.ExitCode)"
    }

    # -------- Locate clang-format.exe --------
    Write-Host "Searching for clang-format.exe in installed files..."
    $clangFormat = Get-ChildItem -Path $installDir -Recurse -Filter "clang-format.exe" |
                   Select-Object -First 1

    if (-not $clangFormat) {
        throw "clang-format.exe not found under $installDir"
    }

    Write-Host "Found clang-format.exe at: $($clangFormat.FullName)"

    # -------- Replace target file --------
    $targetDir = Split-Path $targetPath -Parent
    if (-not (Test-Path $targetDir)) {
        Write-Host "Creating target directory: $targetDir"
        $null = New-Item -ItemType Directory -Path $targetDir -Force
    }

    Write-Host "Copying new clang-format.exe to target..."
    Copy-Item -Path $clangFormat.FullName -Destination $targetPath -Force

    Write-Host ""
    Write-Host " clang-format.exe updated successfully."
    Write-Host "   Version: $LlvmVersion"
    Write-Host "   Location: $targetPath"
}
catch {
    Write-Error " Failed to update clang-format.exe: $_"
}
finally {
    if (Test-Path $tempRoot) {
        Write-Host "Cleaning up temp folder: $tempRoot"
        Remove-Item -Path $tempRoot -Recurse -Force -ErrorAction SilentlyContinue
    }
}
