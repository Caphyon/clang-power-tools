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
    private ICommand addDataCommand;

    public event PropertyChangedEventHandler PropertyChanged;
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

    }


    public string FileExtensions
    {
      get
      {
        return string.IsNullOrWhiteSpace(SettingsModelHandler.FormatSettings.FileExtensions) ? DefaultOptions.FileExtensions : SettingsModelHandler.FormatSettings.FileExtensions;
      }
      set
      {
        SettingsModelHandler.FormatSettings.FileExtensions = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FileExtensions"));
      }
    }

    public string FilesToIgnore
    {
      get
      {
        return string.IsNullOrWhiteSpace(SettingsModelHandler.FormatSettings.FilesToIgnore) ? DefaultOptions.IgnoreFiles : SettingsModelHandler.FormatSettings.FileExtensions;
      }
      set
      {
        SettingsModelHandler.FormatSettings.FilesToIgnore = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FilesToIgnore"));
      }
    }

    public string AssumeFilename
    {
      get
      {
        return SettingsModelHandler.FormatSettings.AssumeFilename;
      }
      set
      {
        SettingsModelHandler.FormatSettings.AssumeFilename = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AssumeFilename"));
      }
    }

    public string CustomExecutable
    {
      get
      {
        return SettingsModelHandler.FormatSettings.CustomExecutable;
      }
      set
      {
        SettingsModelHandler.FormatSettings.CustomExecutable = value;
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
      get { return SettingsModelHandler.FormatSettings.Style; }
      set
      {
        SettingsModelHandler.FormatSettings.Style = value;
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
      get { return SettingsModelHandler.FormatSettings.FallbackStyle; }
      set
      {
        SettingsModelHandler.FormatSettings.FallbackStyle = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedFallbackStyle"));
      }
    }

    public bool FormatOnSave
    {
      get
      {
        return SettingsModelHandler.FormatSettings.FormatOnSave;
      }
      set
      {
        SettingsModelHandler.FormatSettings.FormatOnSave = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FormatOnSave"));
      }
    }
    #endregion
  }
}
