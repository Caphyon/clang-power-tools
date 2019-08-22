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
    public event PropertyChangedEventHandler PropertyChanged;

    private ICommand fileExtensionsAddDataCommand;
    private ICommand filesToIgnoreAddDataCommand;
    private ICommand assumeFilenameAddDataCommand;
    private ICommand customExecutableBrowseCommand;
    #endregion

    #region Constructor
    public FormatSettingsViewModel()
    {
      SettingsViewModelProvider.FormatSettingsViewModel = this;
    }
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
      get => customExecutableBrowseCommand ?? (customExecutableBrowseCommand = new RelayCommand(() => CustomExecutable = OpenFile(string.Empty, ".exe", "Executable files|*.exe"), () => CanExecute));
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
        return string.IsNullOrWhiteSpace(SettingsModelProvider.FormatSettings.FileExtensions) ? DefaultOptions.FileExtensions : SettingsModelProvider.FormatSettings.FileExtensions;
      }
      set
      {
        SettingsModelProvider.FormatSettings.FileExtensions = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FileExtensions"));
      }
    }

    public string FilesToIgnore
    {
      get
      {
        return string.IsNullOrWhiteSpace(SettingsModelProvider.FormatSettings.FilesToIgnore) ? DefaultOptions.IgnoreFiles : SettingsModelProvider.FormatSettings.FilesToIgnore;
      }
      set
      {
        SettingsModelProvider.FormatSettings.FilesToIgnore = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FilesToIgnore"));
      }
    }

    public string AssumeFilename
    {
      get
      {
        return SettingsModelProvider.FormatSettings.AssumeFilename;
      }
      set
      {
        SettingsModelProvider.FormatSettings.AssumeFilename = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AssumeFilename"));
      }
    }

    public string CustomExecutable
    {
      get
      {
        return SettingsModelProvider.FormatSettings.CustomExecutable;
      }
      set
      {
        SettingsModelProvider.FormatSettings.CustomExecutable = value;
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
      get { return SettingsModelProvider.FormatSettings.Style; }
      set
      {
        SettingsModelProvider.FormatSettings.Style = value;
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
      get { return SettingsModelProvider.FormatSettings.FallbackStyle; }
      set
      {
        SettingsModelProvider.FormatSettings.FallbackStyle = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedFallbackStyle"));
      }
    }

    public bool FormatOnSave
    {
      get
      {
        return SettingsModelProvider.FormatSettings.FormatOnSave;
      }
      set
      {
        SettingsModelProvider.FormatSettings.FormatOnSave = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FormatOnSave"));
      }
    }
    #endregion
  }
}
