using ClangPowerTools.MVVM.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace ClangPowerTools
{
  public class FormatSettingsViewModel : CommonSettingsFunctionality, INotifyPropertyChanged
  {
    #region Members
    private ICommand fileExtensionsAddDataCommand;
    private ICommand filesToIgnoreAddDataCommand;
    private ICommand assumeFilenameAddDataCommand;
    private ICommand customExecutableBrowseCommand;

    public event PropertyChangedEventHandler PropertyChanged;
    #endregion

    #region Commands
    public ICommand FileExtensionsAddDataCommand
    {
      get => fileExtensionsAddDataCommand ?? (fileExtensionsAddDataCommand = new RelayCommand(() => FileExtensions = OpenContentDialog(FileExtensions), () => CanExecute));
    }

    public ICommand FilesToIgnoreAddDataCommand
    {
      get => filesToIgnoreAddDataCommand ?? (filesToIgnoreAddDataCommand = new RelayCommand(() => FilesToIgnore = OpenContentDialog(FilesToIgnore), () => CanExecute));
    }

    public ICommand AssumeFilenameAddDataCommand
    {
      get => assumeFilenameAddDataCommand ?? (assumeFilenameAddDataCommand = new RelayCommand(() => AssumeFilename = OpenContentDialog(AssumeFilename), () => CanExecute));
    }

    public ICommand CustomExecutableBrowseCommand
    {
      get => customExecutableBrowseCommand ?? (customExecutableBrowseCommand = new RelayCommand(() => CustomExecutable = BrowseForFile(), () => CanExecute));
    }
    #endregion


    #region Properties
    public bool CanExecute
    {
      get
      {
        return true;
      }
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
        return string.IsNullOrWhiteSpace(SettingsModelHandler.FormatSettings.FilesToIgnore) ? DefaultOptions.IgnoreFiles : SettingsModelHandler.FormatSettings.FilesToIgnore;
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
