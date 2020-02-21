using EnvDTE;
using System.Threading.Tasks;

namespace ClangPowerTools.Commands
{
  public class BackgroundTidyCommand
  {
    #region Members

    public static bool Running { get; set; } = false;

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
      Running = true;
      await TidyCommand.Instance.RunClangTidyAsync(CommandIds.kTidyId, CommandUILocation.Toolbar, document);
      Running = false;
    }

    #endregion
  }
}
