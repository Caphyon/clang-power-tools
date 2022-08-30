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
    private ObservableCollection<AutoCompleteHistoryViewModel> astMatcherFunctions = new();

    private List<AutoCompleteHistoryViewModel> astMatchersConst = new();
    public List<IViewMatcher> ViewMatchers
    {
      get { return FindToolWindowModel.ViewMatchers; }
    }

    //search in ast consts
    public List<AutoCompleteHistoryViewModel> ASTMatchersConst
    {
      get { return astMatchersConst; }
      set
      {
        astMatchersConst = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ASTMatchersConst"));
      }
    }

    //display list
    public ObservableCollection<AutoCompleteHistoryViewModel> ASTMatcherFunctions
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
      astMatcherFunctions = new ObservableCollection<AutoCompleteHistoryViewModel>(GetASTMatchersWithHistory());
      astMatchersConst = new List<AutoCompleteHistoryViewModel>(GetASTMatchersWithHistory());
      this.findToolWindowView = findToolWindowView;
    }
    private List<AutoCompleteHistoryViewModel> GetASTMatchersWithHistory()
    {
      List<AutoCompleteHistoryViewModel> result = new List<AutoCompleteHistoryViewModel>();
      result = ASTMatchers.AutoCompleteMatchers.Select(a => new AutoCompleteHistoryViewModel()
      { RememberAsFavorit = false, Value = a }).ToList();
      return FindToolWindowProvider.AutoCompleteHistory.ToList().Concat(result).ToList();
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
        if (ASTMatchersConst.Find(a => a.Value == matcher.Matchers) is null)
        {
          AutoCompleteHistoryViewModel autoCompleteHistoryViewModel = new AutoCompleteHistoryViewModel
          { RememberAsFavorit = false, Value = matcher.Matchers };

          //add matchers in existing displayed list
          astMatchersConst.Insert(0, autoCompleteHistoryViewModel);
          astMatcherFunctions.Insert(0, autoCompleteHistoryViewModel);

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
}
