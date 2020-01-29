using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClangPowerTools.Commands
{
  public class BackgroundTidyCommand
  {
    #region Members

    public static bool backgroundRun = false;
    private readonly Document document;

    #endregion

    #region Constructor

    public BackgroundTidyCommand(Document document)
    {
      this.document = document;
    }

    #endregion

    #region Public Methods

    public async Task RunClangTidyAsync()
    {
      backgroundRun = true;
      await TidyCommand.Instance.RunClangTidyAsync(CommandIds.kTidyId, CommandUILocation.Toolbar, document);
      backgroundRun = false;
    }


    #endregion
  }
}
