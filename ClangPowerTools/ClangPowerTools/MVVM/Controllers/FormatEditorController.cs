using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace ClangPowerTools
{
  public class FormatEditorController
  {
    #region Methods
    public static Process EditorProcess;

    public static void OpenEditor()
    {
      string vsixPath = Path.GetDirectoryName(typeof(RunClangPowerToolsPackage).Assembly.Location);
      string startInfoArguments = string.Concat(FormatEditorConstants.CommandLineArgument, "\"", Path.Combine(vsixPath, FormatEditorConstants.ExecutableName), "\"");

      try
      {
        EditorProcess = new Process();
        EditorProcess.StartInfo.FileName = FormatEditorConstants.CommandLineExe;
        EditorProcess.StartInfo.Arguments = startInfoArguments;
        EditorProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        EditorProcess.Start();
      }
      catch (Exception e)
      {
        MessageBox.Show(e.Message, FormatEditorConstants.ClangFormatEditor, MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    #endregion
  }
}
