using ClangPowerTools;
using ClangPowerTools.Commands;
using ClangPowerTools.Views;
using ClangPowerToolsShared.Commands;
using ClangPowerToolsShared.MVVM.AutoCompleteHistory;
using ClangPowerToolsShared.MVVM.Constants;
using ClangPowerToolsShared.MVVM.Controllers;
using ClangPowerToolsShared.MVVM.Interfaces;
using ClangPowerToolsShared.MVVM.Models.ToolWindowModels;
using ClangPowerToolsShared.MVVM.Provider;
using System;
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
      FindToolWindowModel = findToolWindowModel;
      this.findToolWindowView = findToolWindowView;
    }

    public void AddPinOnRightPlace(AutoCompleteHistoryModel autoCompleteHistoryModel)
    {
      int itemIndex = astMatchersList.IndexOf(autoCompleteHistoryModel);
      if (autoCompleteHistoryModel.RememberAsFavorit)
      {
        if (itemIndex != -1 && astMatchersList.Count >= itemIndex)
        {
          astMatchersList.Remove(autoCompleteHistoryModel);
          astMatchersList.Insert(0, autoCompleteHistoryModel);
        }
      }
      else
      {
        int lastFindIndex = astMatchersList.ToList().FindLastIndex(a =>
        a.RememberAsFavorit == true && a.Id != autoCompleteHistoryModel.Id);
        if (itemIndex != -1 && lastFindIndex != -1 && astMatchersList.Count >=
          itemIndex && astMatchersList.Count >= lastFindIndex && itemIndex - lastFindIndex != 1)
          Swap(astMatchersList, lastFindIndex, itemIndex);
      }
    }

    public void OnListChange(object sender, TextChangedEventArgs e)
    {
      var tempMatchersList = astMatchersList.Where(a => a.Visibility == UIElementsConstants.Visibile).ToList();
      astMatchersList.Clear();
      foreach (var item in AutoCompleteBehavior.AutocompleteResult)
      {
        var tempItem = tempMatchersList.Where(a => item.Id == a.Id).FirstOrDefault();
        if (tempItem != null)
        {
          tempItem.Value = item.Value;
          if (tempItem.RememberAsFavorit && tempItem.Visibility == UIElementsConstants.Visibile)
            astMatchersList.Insert(0, tempItem);
          else
            astMatchersList.Add(tempItem);
        }
        else
        {
          if (item.RememberAsFavorit && item.Visibility == UIElementsConstants.Visibile)
            astMatchersList.Insert(0, item);
          else
            astMatchersList.Add(item);
        }
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

    private List<AutoCompleteHistoryModel> GetASTMatchersWithHistory()
    {
      List<AutoCompleteHistoryModel> astResult = ASTMatchers.AutoCompleteMatchers.Select(a => new AutoCompleteHistoryModel()
      { RememberAsFavorit = false, Value = a }).ToList();

      if (FindToolWindowProvider.AutoCompleteHistory is null)
        return astResult;
      List<AutoCompleteHistoryModel> jsonResult = FindToolWindowProvider.AutoCompleteHistory
        .Select(a => new AutoCompleteHistoryModel(a)).OrderBy(u => u.RememberAsFavorit ? 0 : 1).ToList();
      return jsonResult.Concat(astResult).ToList();
    }

    private void Swap<T>(IList<T> list, int indexA, int indexB)
    {
      T tmp = list[indexA];
      list[indexA] = list[indexB];
      list[indexB] = tmp;
    }

    private void AddMatcherInHistory()
    {
      if (findToolWindowModel.CurrentViewMatcher.Id == 2)
      {
        var matcher = findToolWindowModel.CurrentViewMatcher as CustomMatchesModel;
        if (ASTMatchersSearchOptions.Find(a => a.Value == matcher.Matchers) is null)
        {
          AutoCompleteHistoryViewModel autoCompleteHistoryViewModel = new AutoCompleteHistoryViewModel
          { Id = Guid.NewGuid().ToString(), RememberAsFavorit = false, Value = matcher.Matchers };

          int indexSearchOptions = astMatchersSearchOptions.ToList().FindLastIndex(a => a.RememberAsFavorit == true);

          //add matchers in existing displayed list
          if (indexSearchOptions > 0)
          {
            astMatchersSearchOptions.Insert(indexSearchOptions + 1, new AutoCompleteHistoryModel(true)
            {
              RememberAsFavorit = false,
              Value = matcher.Matchers,
              Id = autoCompleteHistoryViewModel.Id
            });
          }
          else
          {
            astMatchersSearchOptions.Insert(0, new AutoCompleteHistoryModel(true)
            {
              RememberAsFavorit = false,
              Value = matcher.Matchers,
              Id = autoCompleteHistoryViewModel.Id
            });
          }

          int indexMatchersList = astMatchersList.ToList().FindLastIndex(a => a.RememberAsFavorit == true);
          if (indexMatchersList > 0)
          {
            astMatchersList.Insert(indexMatchersList + 1, new AutoCompleteHistoryModel(autoCompleteHistoryViewModel, true));
          }
          else
          {
            astMatchersList.Insert(0, new AutoCompleteHistoryModel(autoCompleteHistoryViewModel, true));
          }

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
