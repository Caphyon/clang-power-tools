using ClangPowerTools.MVVM.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ClangPowerTools
{
  public class FormatSettingsViewModel : INotifyPropertyChanged
  {
    #region Members
    private FormatSettingsModel formatSettings = new FormatSettingsModel();
    private const string GeneralSettingsFileName = "FormatConfiguration.config";
    private string path = string.Empty;
    private ICommand addDataCommand;

    public event PropertyChangedEventHandler PropertyChanged;
    #endregion

    #region Constructors
    public FormatSettingsViewModel()
    {
      SettingsPathBuilder settingsPathBuilder = new SettingsPathBuilder();
      path = settingsPathBuilder.GetPath(GeneralSettingsFileName);
      CPTSettings.FormatSettings = formatSettings;
    }
    #endregion


    #region Properties
    public ICommand AddDataCommand
    {
      get => addDataCommand ?? (addDataCommand = new RelayCommand(() => OpenDataDialog(), () => CanExecute));
    }

    public bool CanExecute
    {
      get
      {
        return true;
      }
    }

    public void OpenDataDialog()
    {
      MessageBox.Show("Hello, world!");
      CPTSettings cPTSettings = new CPTSettings();
      cPTSettings.CheckOldSettings();
    }


    public string FileExtensions
    {
      get
      {
        return string.IsNullOrWhiteSpace(formatSettings.FileExtensions) ? DefaultOptions.FileExtensions : formatSettings.FileExtensions;
      }
      set
      {
        formatSettings.FileExtensions = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FileExtensions"));
      }
    }

    public string FilesToIgnore
    {
      get
      {
        return string.IsNullOrWhiteSpace(formatSettings.FilesToIgnore) ? DefaultOptions.IgnoreFiles : formatSettings.FileExtensions;
      }
      set
      {
        formatSettings.FilesToIgnore = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FilesToIgnore"));
      }
    }

    public string AssumeFilename
    {
      get
      {
        return formatSettings.AssumeFilename;
      }
      set
      {
        formatSettings.AssumeFilename = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AssumeFilename"));
      }
    }

    public string CustomExecutable
    {
      get
      {
        return formatSettings.CustomExecutable;
      }
      set
      {
        formatSettings.CustomExecutable = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CustomExecutable"));
      }
    }

    public IEnumerable<ClangFormatStyle> Styles
    {
      get
      {
        return Enum.GetValues(typeof(ClangFormatStyle)).Cast<ClangFormatStyle>();
      }
    }

    public ClangFormatStyle SelectedStyle
    {
      get { return formatSettings.Style; }
      set
      {
        formatSettings.Style = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedStyle"));
      }
    }

    public IEnumerable<ClangFormatFallbackStyle> FallbackStyles
    {
      get
      {
        return Enum.GetValues(typeof(ClangFormatFallbackStyle)).Cast<ClangFormatFallbackStyle>();
      }
    }

    public ClangFormatFallbackStyle SelectedFallbackStyle
    {
      get { return formatSettings.FallbackStyle; }
      set
      {
        formatSettings.FallbackStyle = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedFallbackStyle"));
      }
    }

    public bool FormatOnSave
    {
      get
      {
        return formatSettings.FormatOnSave;
      }
      set
      {
        formatSettings.FormatOnSave = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FormatOnSave"));
      }
    }
    #endregion
  }
}
