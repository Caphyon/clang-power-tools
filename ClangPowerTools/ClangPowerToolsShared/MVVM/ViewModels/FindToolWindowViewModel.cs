using ClangPowerTools;
using ClangPowerTools.Commands;
using ClangPowerTools.Views;
using ClangPowerToolsShared.Commands;
using ClangPowerToolsShared.MVVM.AutoCompleteHistory;
using ClangPowerToolsShared.MVVM.Controllers;
using ClangPowerToolsShared.MVVM.Interfaces;
using ClangPowerToolsShared.MVVM.Models.ToolWindowModels;
using ClangPowerToolsShared.MVVM.Provider;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;

namespace ClangPowerToolsShared.MVVM.ViewModels
{

  public class FindToolWindowViewModel : FindController
  {
    public event PropertyChangedEventHandler PropertyChanged;
    private ObservableCollection<string> astMatcherFunctions = new();

    private List<string> astMatchersConst = new();
    public List<string> ASTMatchersConst
    {
      get { return astMatchersConst; }
      set
      {
        astMatchersConst = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ASTMatchersConst"));
      }
    }


    public List<IViewMatcher> ViewMatchers
    {
      get { return FindToolWindowModel.ViewMatchers; }
    }

    public ObservableCollection<string> ASTMatcherFunctions
    {
      get { return astMatcherFunctions; }
      set
      {
        astMatcherFunctions = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ASTMatcherFunctions"));
      }
    }

    public FindToolWindowViewModel(FindToolWindowView findToolWindowView)
    {
      AutoCompleteBehavior.OnListUpdate += OnListChange;
      astMatcherFunctions = new ObservableCollection<string>(GetASTMatchersWithHistory());
      astMatchersConst = new List<string>(GetASTMatchersWithHistory());
      this.findToolWindowView = findToolWindowView;
    }

    private List<string> GetASTMatchersWithHistory()
    {
      return FindToolWindowProvider.AutoCompleteHistory.Select(a => a.Value).ToList().Concat(ASTMatchers.AutoCompleteMatchers).ToList();
    }

    public void OnListChange(object sender, TextChangedEventArgs e)
    {
      astMatcherFunctions.Clear();
      foreach (var item in AutoCompleteBehavior.AutocompleteResult)
      {
        astMatcherFunctions.Add(item);
      }
      ASTMatcherFunctions = astMatcherFunctions;
    }

    public void OpenToolWindow() { }

    public void RunQuery()
    {
      if (!RunController.StopCommandActivated)
      {
        SelectCommandToRun(findToolWindowModel.CurrentViewMatcher);
        RunPowershellQuery();
      }
      AfterCommand();
    }

    public void SelectCommandToRun(IViewMatcher viewMatcher)
    {
      findToolWindowModel.UpdateUiToSelectedModel(viewMatcher);
      FindToolWindowModel = findToolWindowModel;
    }

    public void RunCommandFromView()
    {
      BeforeCommand();
      LaunchCommand();
      //add in history
      AddMatcherInHistory();
      CommandControllerInstance.CommandController.LaunchCommandAsync(CommandIds.kClangFindRun, CommandUILocation.ContextMenu);
    }

    private void AddMatcherInHistory()
    {
      if (findToolWindowModel.CurrentViewMatcher.Id == 2)
      {
        var matcher = findToolWindowModel.CurrentViewMatcher as CustomMatchesModel;
        AutoCompleteHistoryViewModel autoCompleteHistoryViewModel = new AutoCompleteHistoryViewModel
        { Name = matcher.Name, RememberAsFavorit = false, Value = matcher.Matchers };

        if (ASTMatchersConst.Contains(matcher.Matchers))
          return;

        //add matchers in existing displayed list
        astMatchersConst.Insert(0,matcher.Matchers);
        astMatcherFunctions.Add(matcher.Matchers);

        //save matchers displayed list
        ASTMatcherFunctions = astMatcherFunctions;
        ASTMatchersConst = astMatchersConst;

        //save matchers on json history file
        FindToolWindowProvider.AddAutoCompleteHistory(autoCompleteHistoryViewModel);
        FindToolWindowHandler findToolWindowHandler = new FindToolWindowHandler();
        findToolWindowHandler.SaveMatchersHistoryData();
      }
    }
  }
}
