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

    private FormatSettingsModel formatModel;
    private ICommand fileExtensionsAddDataCommand;
    private ICommand filesToIgnoreAddDataCommand;
    private ICommand assumeFilenameAddDataCommand;
    private ICommand customExecutableBrowseCommand;
    #endregion

    #region Properties
    public FormatSettingsModel FormatModel
    {
      get
      {
        return formatModel;
      }
      set
      {
        formatModel = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FormatModel"));
      }
    }


    public bool CanExecute
    {
      get
      {
        return true;
      }
    }
    #endregion

    #region Constructor
    public FormatSettingsViewModel()
    {
      formatModel = new FormatSettingsModel();
      SettingsViewModelProvider.FormatSettingsViewModel = this;
    }

    public IEnumerable<ClangFormatStyle> StyleItems
    {
      get
      {
        return Enum.GetValues(typeof(ClangFormatStyle)).Cast<ClangFormatStyle>();
      }
    }

    public IEnumerable<ClangFormatFallbackStyle> FallBackStyleItems
    {
      get
      {
        return Enum.GetValues(typeof(ClangFormatFallbackStyle)).Cast<ClangFormatFallbackStyle>();
      }
    }
  #endregion

  #region Commands
  public ICommand FileExtensionsAddDataCommand
    {
      get => fileExtensionsAddDataCommand ?? (fileExtensionsAddDataCommand = new RelayCommand(() => UpdateFileExtensions(), () => CanExecute));
    }

    public ICommand FilesToIgnoreAddDataCommand
    {
      get => filesToIgnoreAddDataCommand ?? (filesToIgnoreAddDataCommand = new RelayCommand(() => UpdateFilesToIgnore(), () => CanExecute));
    }

    public ICommand AssumeFilenameAddDataCommand
    {
      get => assumeFilenameAddDataCommand ?? (assumeFilenameAddDataCommand = new RelayCommand(() => UpdateAssumeFilename(), () => CanExecute));
    }

    public ICommand CustomExecutableBrowseCommand
    {
      get => customExecutableBrowseCommand ?? (customExecutableBrowseCommand = new RelayCommand(() => UpdateCustomExecutable(), () => CanExecute));
    }
    #endregion

    #region Methods
    private void UpdateFileExtensions()
    {
      formatModel.FileExtensions = OpenContentDialog(formatModel.FileExtensions);
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FormatModel"));
    }

    private void UpdateFilesToIgnore()
    {
      formatModel.FilesToIgnore = OpenContentDialog(formatModel.FilesToIgnore);
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FormatModel"));
    }

    private void UpdateAssumeFilename()
    {
      formatModel.AssumeFilename = OpenContentDialog(formatModel.AssumeFilename);
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FormatModel"));
    }

    private void UpdateCustomExecutable()
    {
      formatModel.CustomExecutable = OpenFile(string.Empty, ".exe", "Executable files|*.exe");
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FormatModel"));
    }

    #endregion
  }
}
