using ClangPowerTools;
using ClangPowerTools.Views;
using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using ClangPowerToolsShared.MVVM.Models.ToolWindowModels;
using ClangPowerTools.MVVM.Command;

namespace ClangPowerToolsShared.MVVM.ViewModels
{

  public class FindToolWindowViewModel : CommonSettingsFunctionality, INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;
    private FindToolWindowView findToolWindowView;
    private FindToolWindowModel findToolWindowModel = new();
    private ICommand matchCommand;

    public FindToolWindowViewModel(FindToolWindowView findToolWindowView)
    {
      this.findToolWindowView = findToolWindowView;
    }

    public FindToolWindowModel FindToolWindowModel
    {
      get { return findToolWindowModel; }
      set
      {
        findToolWindowModel = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FindToolWindowModel"));
      }
    }

    public bool CanExecute
    {
      get
      {
        return true;
      }
    }

    public ICommand MatchCommand
    {
      get => matchCommand ?? (matchCommand = new RelayCommand(() => Match(), () => CanExecute));
    }

    public void Match()
    {


    }

    public void OpenToolWindow(List<string> filesPath)
    {

    }
  }
}
