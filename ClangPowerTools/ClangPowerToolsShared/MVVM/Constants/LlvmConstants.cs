﻿namespace ClangPowerTools.MVVM.Constants
{
  public static class LlvmConstants
  {
    public const string InstallExeParameters = "/S /D=";
    public const string UninstallExeParameters = "/S";
    public const string Arguments = @"/C reg delete HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\LLVM /f &";
    public const string ProcessFileName = "cmd.exe";
    public const string ProcessVerb = "runas";
    public const string ReleasesUri = @"http://releases.llvm.org";
    public const string GitHubUri = @"https://github.com/llvm/llvm-project/releases/download";
    public const string Llvm = "LLVM";
    public const string Uninstall = "Uninstall";
    public const string Os64Paramater = "-win64.exe";
    public const string Os32Paramater = "-win32.exe";
  }
}
