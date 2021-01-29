using ClangPowerTools.MVVM;
using ClangPowerTools.MVVM.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
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
    private ICommand openClangFormatEditorCommand;
    #endregion

    #region Constructor

    public FormatSettingsViewModel()
    {
      formatModel = SettingsProvider.FormatSettingsModel;
    }

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

    public SettingsTooltips Tooltip { get; } = new SettingsTooltips();

    #endregion

    #region Commands

    public ICommand FileExtensionsAddDataCommand
    {
      get => fileExtensionsAddDataCommand ??= new RelayCommand(() => UpdateFileExtensions(), () => CanExecute);
    }

    public ICommand FilesToIgnoreAddDataCommand
    {
      get => filesToIgnoreAddDataCommand ??= new RelayCommand(() => UpdateFilesToIgnore(), () => CanExecute);
    }

    public ICommand AssumeFilenameAddDataCommand
    {
      get => assumeFilenameAddDataCommand ??= new RelayCommand(() => UpdateAssumeFilename(), () => CanExecute);
    }

    public ICommand CustomExecutableBrowseCommand
    {
      get => customExecutableBrowseCommand ??= new RelayCommand(() => UpdateCustomExecutable(), () => CanExecute);
    }

    public ICommand OpenClangFormatEditorCommand
    {
      get => openClangFormatEditorCommand ??= new RelayCommand(() => OpenClangFormatEditor(), () => CanExecute);
    }

    #endregion

    #region Methods

    private void OpenClangFormatEditor()
    {
      SettingsProvider.SettingsView.Close();
      string vsixPath = Path.GetDirectoryName(typeof(RunClangPowerToolsPackage).Assembly.Location);

      try
      {
        var process = new Process();
        process.StartInfo.FileName = Path.Combine(vsixPath, "ClangFormatEditor.exe");
        process.StartInfo.WorkingDirectory = vsixPath;

        process.Start();
      }
      catch (Exception e)
      {
        MessageBox.Show(e.Message, "Clang Format Editor", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

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
