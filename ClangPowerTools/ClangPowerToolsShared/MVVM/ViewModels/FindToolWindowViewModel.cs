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
    private ObservableCollection<AutoCompleteHistoryModel> astMatchersList = new();

    private List<AutoCompleteHistoryModel> astMatchersSearchOptions = new();
    public List<IViewMatcher> ViewMatchers
    {
      get { return FindToolWindowModel.ViewMatchers; }
    }

    //search in ast consts
    public List<AutoCompleteHistoryModel> ASTMatchersSearchOptions
    {
      get { return astMatchersSearchOptions; }
      set
      {
        astMatchersSearchOptions = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ASTMatchersSearchOptions"));
      }
    }

    //display list
    public ObservableCollection<AutoCompleteHistoryModel> ASTMatchersList
    {
      get { return astMatchersList; }
      set
      {
        astMatchersList = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ASTMatchersList"));
      }
    }

    public FindToolWindowViewModel(FindToolWindowView findToolWindowView)
    {
      AutoCompleteBehavior.OnListUpdate += OnListChange;
      astMatchersList = new ObservableCollection<AutoCompleteHistoryModel>(GetASTMatchersWithHistory());
      astMatchersSearchOptions = new List<AutoCompleteHistoryModel>(GetASTMatchersWithHistory());
      this.findToolWindowView = findToolWindowView;
    }

    private List<AutoCompleteHistoryModel> GetASTMatchersWithHistory()
    {
      List<AutoCompleteHistoryModel> astResult = ASTMatchers.AutoCompleteMatchers.Select(a => new AutoCompleteHistoryModel()
      { RememberAsFavorit = false, Value = a }).ToList();
      List<AutoCompleteHistoryModel> jsonResult = FindToolWindowProvider.AutoCompleteHistory.Select(a => new AutoCompleteHistoryModel(a)).ToList();
      return jsonResult.Concat(astResult).ToList();
    }

    public void OnListChange(object sender, TextChangedEventArgs e)
    {
      astMatchersList.Clear();
      foreach (var item in AutoCompleteBehavior.AutocompleteResult)
      {
        astMatchersList.Add(item);
      }
      ASTMatchersList = astMatchersList;
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
        if (ASTMatchersSearchOptions.Find(a => a.Value == matcher.Matchers) is null)
        {
          AutoCompleteHistoryViewModel autoCompleteHistoryViewModel = new AutoCompleteHistoryViewModel
          { RememberAsFavorit = false, Value = matcher.Matchers };

          //add matchers in existing displayed list
          astMatchersSearchOptions.Insert(0, new AutoCompleteHistoryModel() { RememberAsFavorit = false, Value = matcher.Matchers });
          astMatchersList.Insert(0, new AutoCompleteHistoryModel(autoCompleteHistoryViewModel));

          //save matchers displayed list
          ASTMatchersList = astMatchersList;
          ASTMatchersSearchOptions = astMatchersSearchOptions;

          //save matchers on json history file
          FindToolWindowProvider.AddAutoCompleteHistory(autoCompleteHistoryViewModel);
          FindToolWindowHandler findToolWindowHandler = new FindToolWindowHandler();
          findToolWindowHandler.SaveMatchersHistoryData();
        }
      }
    }
  }
}
