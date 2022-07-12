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
    
    public FindControllerModel()
    {
      viewMatchers.Add(DefaultArgsModel);
      viewMatchers.Add(CustomMatchesModel);

      matcherDetails = DefaultArgsModel.Details;
    }

    public void HideModelsOptions()
    {
      foreach (var matche in viewMatchers)
      {
        matche.Hide();
      }
    }



  }
}
