using ClangPowerTools.MVVM.Views;

namespace ClangPowerTools.MVVM.Controllers
{
  public class DiffController
  {
    #region Members

    Formatter formatter;

    #endregion


    #region Constructor

    public DiffController(Formatter formatter)
    {
      // TODO use to renamed class FormatEditorController => 
    }

    #endregion

    #region Methods

    public void CreateConfigUsingCompare(string editorInput, string editorOutput)
    {
      var diffMatchPatchWrapper = new DiffMatchPatchWrapper();
      diffMatchPatchWrapper.Diff(editorInput, editorOutput);
      diffMatchPatchWrapper.CleanupEfficiency();
      diffMatchPatchWrapper.CleanupSemantic();

      //var diffLev = diffMatchPatchWrapper.DiffLevenshtein();
      var html = diffMatchPatchWrapper.DiffAsHtml();

      var diffWindow = new DiffWindow(html);
      diffWindow.Show();
    }

    #endregion
  }
}
