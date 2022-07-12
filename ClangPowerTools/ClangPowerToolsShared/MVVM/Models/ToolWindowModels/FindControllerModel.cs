using ClangPowerToolsShared.Commands;
using ClangPowerToolsShared.MVVM.Interfaces;
using System.Collections.Generic;

namespace ClangPowerToolsShared.MVVM.Models.ToolWindowModels
{
  public class FindControllerModel
  {
    protected string matcherDetails = string.Empty;

    public DefaultArgsModel DefaultArgsModel { get; set; } = new DefaultArgsModel();
    public CustomMatchesModel CustomMatchesModel { get; set; } = new CustomMatchesModel();

    public List<IViewMatche> viewMatchers = new List<IViewMatche>();
    protected int currentCommandId = 0;
    public FindControllerModel()
    {
      currentCommandId = FindCommandIds.kDefaultArgsId;
      viewMatchers.Add(DefaultArgsModel);
      viewMatchers.Add(CustomMatchesModel);

      matcherDetails = DefaultArgsModel.Details;
    }

    public void HideAllModelsOptions()
    {
      foreach (var matche in viewMatchers)
      {
        matche.Hide();
      }
    }

    protected void HidePreviousSelectedModel()
    {
      switch (currentCommandId)
      {
        case FindCommandIds.kDefaultArgsId:
          {
            DefaultArgsModel.Hide();
            break;
          }
        case FindCommandIds.kCustomMatchesId:
          {
            CustomMatchesModel.Hide();
            break;
          }
        default:
          break;
      }
    }

    protected void ShowSelectedModel(int commandId)
    {
      currentCommandId = commandId;
      switch (currentCommandId)
      {
        case FindCommandIds.kDefaultArgsId:
          {
            DefaultArgsModel.Show();
            break;
          }
        case FindCommandIds.kCustomMatchesId:
          {
            CustomMatchesModel.Show();
            break;
          }
        default:
          break;
      }
    }

  }
}
