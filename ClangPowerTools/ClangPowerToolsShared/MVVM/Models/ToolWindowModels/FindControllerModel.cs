using ClangPowerToolsShared.Commands;
using ClangPowerToolsShared.MVVM.Interfaces;
using System.Collections.Generic;

namespace ClangPowerToolsShared.MVVM.Models.ToolWindowModels
{
  public class FindControllerModel
  {
    protected string matcherDetails = string.Empty;

    public DefaultArgsModel DefaultArgsModel { get; set; }
    public CustomMatchesModel CustomMatchesModel { get; set; }
    public List<IViewMatcher> ViewMatchers;
    protected IViewMatcher currentViewMatcher;

    public FindControllerModel()
    {
      DefaultArgsModel = new DefaultArgsModel();
      CustomMatchesModel = new CustomMatchesModel();
      ViewMatchers = new List<IViewMatcher>();

      ViewMatchers.Add(DefaultArgsModel);
      ViewMatchers.Add(CustomMatchesModel);

      currentViewMatcher = DefaultArgsModel;
      ShowSelectedModel(currentViewMatcher);
    }


    protected void HidePreviousSelectedModel()
    {
      currentViewMatcher.Hide();
    }

    protected void ShowSelectedModel(IViewMatcher viewMatcher)
    {
      currentViewMatcher = viewMatcher;
      currentViewMatcher.Show();
    }
  }
}
