using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClangPowerTools.Commands;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Xunit;

namespace ClangPowerTools.Tests
{
  [VsTestSettings(UIThread = true)]
  public class CommandTests
  {
    #region Members

    private const string kCommandsGuid = "498fdff5-5217-4da9-88d2-edad44ba3874";

    private CompileCommand mCompileCommand;
    private TidyCommand mTidyCommand;
    private ClangFormatCommand mFormatCommand;

    private StopClang mStopCommand;

    private IgnoreCompileCommand mIgnoreCompileCommand;
    private IgnoreFormatCommand mIgnoreFormatCommand;

    #endregion

    [VsFact(Version = "2019")]
    public async System.Threading.Tasks.Task GetCommandsAsync()
    {
      await UnitTestUtility.LoadPackageAsync();
      var dte = (DTE2)ServiceProvider.GlobalProvider.GetService(typeof(DTE));

      var compileCommand = CompileCommand.Instance;
      await compileCommand.RunClangCompileAsync(CommandIds.kCompileId, CommandUILocation.ContextMenu);

      var script = CommandTestUtility.ScriptCommand;


      //var success = UnitTestUtility.RunCommand(dte, kCommandsGuid);
      //if (!success)
      //  Assert.True(success);



    }



  }
}
