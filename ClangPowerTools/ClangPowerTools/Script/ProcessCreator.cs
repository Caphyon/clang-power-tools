using ClangPowerTools.DialogPages;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace ClangPowerTools.Script
{
  public class ProcessCreator
  {
    public Process New(ClangFormatPage aClangFormatPage, string text, int offset, int length, string path, string filePath)
    {
      string assemblyPath = Assembly.GetExecutingAssembly().Location;
      assemblyPath = assemblyPath.Substring(0, assemblyPath.LastIndexOf('\\'));

      string clangFormatPath = Path.Combine(assemblyPath, ScriptConstants.kClangFormat);

      Process process = new Process();
      process.StartInfo.CreateNoWindow = true;
      process.StartInfo.RedirectStandardInput = true;
      process.StartInfo.RedirectStandardOutput = true;
      process.StartInfo.RedirectStandardError = true;
      process.StartInfo.UseShellExecute = false;
      process.StartInfo.FileName = clangFormatPath;

      //string style = aClangFormatPage.Style.Replace("\"", "\\\"");
      //string fallbackStyle = aClangFormatPage.FallbackStyle.Replace("\"", "\\\"");

      process.StartInfo.Arguments =
        //$" -offset " + offset +
        //$" -length " + length +
        //$" -output-replacements-xml " +
         " -i " +
        $" {ScriptConstants.kStyle} \"{aClangFormatPage.Style}\"" +
        $" {ScriptConstants.kFallbackStyle} \"{aClangFormatPage.FallbackStyle}\"" +
        $" \"{filePath}\"";

      string assumeFilename = aClangFormatPage.AssumeFilename;
      //if (string.IsNullOrEmpty(assumeFilename))
      //  assumeFilename = filePath;
      if (!string.IsNullOrEmpty(assumeFilename))
        process.StartInfo.Arguments += $" {ScriptConstants.kAssumeFilename} \"{assumeFilename}\"";

      if (path != null)
        process.StartInfo.WorkingDirectory = path;

      return process;
    }


  }
}
