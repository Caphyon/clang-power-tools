using ClangPowerTools.Helpers;
using ClangPowerTools.MVVM.Commands;
using ClangPowerTools.MVVM.Interfaces;
using ClangPowerTools.MVVM.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Process = System.Diagnostics.Process;

namespace ClangPowerTools
{
  public class FormatStyleViewModel : CommonSettingsFunctionality, INotifyPropertyChanged
  {
    #region Members

    public event PropertyChangedEventHandler PropertyChanged;

    private readonly SettingsPathBuilder settingsPathBuilder = new SettingsPathBuilder();
    private readonly FormatOptionsView formatOptionsView;
    private ICommand createFormatFileCommand;
    private ICommand formatCodeCommand;
    private ICommand resetCommand;
    private ICommand openUri;
    private IFormatOption selectedOption;
    List<IFormatOption> formatOptions = new FormatOptionsData().FormatOptions;
    #endregion

    #region Constructor

    public FormatStyleViewModel(FormatOptionsView formatOptionsView)
    {
      selectedOption = FormatOptions.First();
      this.formatOptionsView = formatOptionsView;
    }

    #endregion

    #region Properties

    public List<IFormatOption> FormatOptions
    {
      get
      {
        return formatOptions;
      }
      set
      {
        formatOptions = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FormatOptions"));
        RunFormat();
      }
    }

    public IFormatOption SelectedOption
    {
      get
      {
        return selectedOption;
      }
      set
      {
        selectedOption = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedOption"));
      }
    }

    public IEnumerable<ClangFormatStyle> PredefinedStyles
    {
      get
      {
        return Enum.GetValues(typeof(ClangFormatStyle)).Cast<ClangFormatStyle>();
      }
    }

    public IEnumerable<ToggleValues> BooleanComboboxValues
    {
      get
      {
        return Enum.GetValues(typeof(ToggleValues)).Cast<ToggleValues>();
      }
    }

    public ClangFormatStyle SelectedPredefinedStyle { get; set; } = ClangFormatStyle.file;


    public bool CanExecute
    {
      get
      {
        return true;
      }
    }

    #endregion


    #region Commands

    public ICommand CreateFormatFileCommand
    {
      get => createFormatFileCommand ?? (createFormatFileCommand = new RelayCommand(() => CreateFormatFile(), () => CanExecute));
    }

    public ICommand FormatCodeCommand
    {
      get => formatCodeCommand ?? (formatCodeCommand = new RelayCommand(() => RunFormat(), () => CanExecute));
    }

    public ICommand OpenClangFormatUriCommand
    {
      get => openUri ?? (openUri = new RelayCommand(() => OpenUri("https://clang.llvm.org/docs/ClangFormatStyleOptions.html"), () => CanExecute));
    }

    public ICommand ResetCommand
    {
      get => resetCommand ?? (resetCommand = new RelayCommand(() => ResetOptions(), () => CanExecute));
    }


    #endregion


    #region Methods


    private void OpenUri(string uri)
    {
      Process.Start(new ProcessStartInfo(uri));
    }

    private void ResetOptions()
    {
      FormatOptions = new FormatOptionsData().FormatOptions;
    }

    private void CreateFormatFile()
    {
      string fileName = ".clang-format";
      string defaultExt = ".clang-format";
      string filter = "Configuration files (.clang-format)|*.clang-format";

      string path = SaveFile(fileName, defaultExt, filter);
      if (string.IsNullOrEmpty(path) == false)
      {
        WriteContentToFile(path, FormatOptionFile.CreateOutput(formatOptions).ToString());
      }
    }

    private void RunFormat()
    {
      var text = formatOptionsView.CodeEditor.Text;
      string filePath = Path.Combine(settingsPathBuilder.GetPath(""), "FormatTemp.cpp");
      string formatFilePath = Path.Combine(settingsPathBuilder.GetPath(""), ".clang-format");

      WriteContentToFile(formatFilePath, FormatOptionFile.CreateOutput(formatOptions).ToString());
      WriteContentToFile(filePath, text);

      var content = FormatFileOutsideProject(settingsPathBuilder.GetPath(""), filePath);
      formatOptionsView.CodeEditor.Text = content;

      FileSystem.DeleteFile(filePath);
      FileSystem.DeleteFile(formatFilePath);
    }

    public static string FormatFileOutsideProject(string path, string filePath)
    {
      string vsixPath = Path.GetDirectoryName(
        typeof(RunClangPowerToolsPackage).Assembly.Location);

      Process process = new Process();
      process.StartInfo.UseShellExecute = false;
      process.StartInfo.CreateNoWindow = true;
      process.StartInfo.RedirectStandardInput = true;
      process.StartInfo.RedirectStandardOutput = true;
      process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
      process.StartInfo.RedirectStandardError = true;
      process.StartInfo.FileName = Path.Combine(vsixPath, ScriptConstants.kClangFormat);
      process.StartInfo.WorkingDirectory = path;
      process.StartInfo.Arguments = $"-style=file \"{Path.GetFullPath(filePath)}\"";

      process.Start();
      var output = process.StandardOutput.ReadToEnd();
      process.WaitForExit();
      process.Close();

      return output;
    }

    #endregion
  }
}
