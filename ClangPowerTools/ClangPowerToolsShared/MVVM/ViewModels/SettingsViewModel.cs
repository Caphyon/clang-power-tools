﻿using ClangPowerTools.MVVM.Commands;
using ClangPowerTools.MVVM.Views;
using ClangPowerTools.Views;
using System;
using System.Windows.Input;

namespace ClangPowerTools
{
  public class SettingsViewModel
  {
    #region Members

    private ICommand upgradeCommand;

    private readonly SettingsHandler settingsHandler = new SettingsHandler();
    private readonly SettingsView settingsView;

    private const int PERSONAL_LICENSE_HEIGTH = 535;
    private const int NO_ACCOUNT_HEIGTH = 600;

    #endregion

    #region Constructors

    public SettingsViewModel(SettingsView settingsView, bool showFooter)
    {
      this.settingsView = settingsView;
      settingsView.Closed += OnClosed;
      ShowFooter = showFooter;
      Heigth = ShowFooter ? PERSONAL_LICENSE_HEIGTH : NO_ACCOUNT_HEIGTH;
    }

    #endregion

    #region Properties

    public bool ShowFooter { get; private set; }

    public int Heigth { get; set; }

    public bool CanExecute
    {
      get
      {
        return true;
      }
    }

    public ICommand LogIn
    {
      get => upgradeCommand ??= new RelayCommand(() => LogInAction(), () => CanExecute);
    }

    #endregion

    #region Public Methods

    public void OnClosed(object sender, EventArgs e)
    {
      settingsHandler.SaveSettings();
      settingsView.Closed -= OnClosed;
      SettingsHandler.RefreshSettingsView = null;
    }

    #endregion

    #region Private Methods

    private void LogInAction()
    {
      settingsView.Close();
      LoginView loginView = new LoginView();
      loginView.ShowDialog();
    }

    #endregion
  }
}
