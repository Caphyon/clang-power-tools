using ClangPowerToolsShared.MVVM.Constants;
using ClangPowerToolsShared.MVVM.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;

namespace ClangPowerToolsShared.MVVM.Models.ToolWindowModels
{
  public class FindToolWindowModel : FindControllerModel, INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;
    private bool isRunning = false;
    
    public FindToolWindowModel()
    {
      HideProgressBar();
    }

    public void UpdateUiToSelectedModel(IViewMatcher viewMatcher)
    {
      HidePreviousSelectedModel();
      ShowSelectedModel(viewMatcher);
      CurrentViewMatcher = currentViewMatcher;
    }

    private string progressBarVisibility;
    public string ProgressBarVisibility
    {
      get { return progressBarVisibility; }
      set
      {
        progressBarVisibility = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ProgressBarVisibility"));
      }
    }

    public IViewMatcher CurrentViewMatcher
    {
      get { return currentViewMatcher; }
      set
      {
        currentViewMatcher = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentViewMatcher"));
      }
    }

    public bool IsEnabled
    {
      get { return !IsRunning; }
      set
      {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsEnabled"));
      }
    }

    public bool IsRunning
    {
      get
      {
        return isRunning;
      }
      set
      {
        if (value)
          ShowProgressBar();
        else
          HideProgressBar();
        isRunning = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsRunning"));
      }
    }

    private void ShowProgressBar()
    {
      ProgressBarVisibility = UIElementsConstants.Visibile;
    }

    private void HideProgressBar()
    {
      ProgressBarVisibility = UIElementsConstants.Hidden;
    }
  }
}
