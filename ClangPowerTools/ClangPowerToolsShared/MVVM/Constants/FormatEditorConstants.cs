using System;
using System.IO;

namespace ClangPowerTools
{
  public static class FormatEditorConstants
  {
    public const string ExecutableName = "ClangFormatEditor.exe";
    public const string ClangFormatEditorFolder = "ClangFormatEditor";
    public const string UpdaterParameter = " VisualStudio";
    public const string ClangFormatEditor = "Clang Format Editor";
    public const string FrameworkdUrlDownload = @"https://dotnet.microsoft.com/download/dotnet/thank-you/runtime-desktop-5.0.2-windows-x86-installer";
    public const string FrameworkPath = @"C:\Program Files (x86)\dotnet\shared\Microsoft.WindowsDesktop.App";
    public const string ClangFormatExe = "clang-format.exe";
    public const string ClangFormatMsi = "Clang Format Editor.msi";
    public const string SetupFailed = "Clang-Format Setup Failed";
    public static string ClangFormatEditorPath { get; } = Path.Combine(Environment.ExpandEnvironmentVariables("%ProgramFiles(x86)%"), "Caphyon", ClangFormatEditor);
  }
}
