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

    private readonly SettingsPathBuilder settingsPathBuilder;
    private readonly FormatOptionsView formatOptionsView;
    private FormatOptionsData formatOptionsData;
    private ICommand selctCodeFileCommand;
    private ICommand createFormatFileCommand;
    private ICommand formatCodeCommand;
    private ICommand resetCommand;
    private ICommand openUri;
    private IFormatOption selectedOption;
    private List<IFormatOption> formatStyleOptions;
    private EditorStyles editorStyles = EditorStyles.Custom;
    private bool isCustomStyle = true;
    private string nameColumnWidth;
    private const string autoSize = "auto";
    private const string nameColumnWidthMax = "340";
    public const string FileExtensionsSelectFile = "Code files (*.c;*.cpp;*.cxx;*.cc;*.tli;*.tlh;*.h;*.hh;*.hpp;*.hxx;*.h;*.h;*.h)|*.c;*.cpp;*.cxx;*.cc;*.tli;*.tlh;*.h;*.hh;*.hpp;*.hxx";
    #endregion

    #region Constructor

    public FormatStyleViewModel(FormatOptionsView formatOptionsView)
    {
      settingsPathBuilder = new SettingsPathBuilder();
      this.formatOptionsView = formatOptionsView;
      InitializeStyleOptions();
    }

    #endregion

    #region Properties

    public List<IFormatOption> FormatOptions
    {
      get
      {
        return formatStyleOptions;
      }
      set
      {
        formatStyleOptions = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FormatOptions"));
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

    public IEnumerable<EditorStyles> Styles
    {
      get
      {
        return Enum.GetValues(typeof(EditorStyles)).Cast<EditorStyles>();
      }
    }

    public IEnumerable<ToggleValues> BooleanComboboxValues
    {
      get
      {
        return Enum.GetValues(typeof(ToggleValues)).Cast<ToggleValues>();
      }
    }

    public EditorStyles SelectedStyle
    {
      get 
      { 
        return editorStyles; 
      }
      set 
      { 
        editorStyles = value; 
        if(editorStyles == EditorStyles.Custom)
        {
          isCustomStyle = true;
          NameColumnWidth = autoSize;
          EnableOptionColumnWidth = autoSize;
        }
        else
        {
          NameColumnWidth = nameColumnWidthMax;
          EnableOptionColumnWidth = "0";
          FormatOptions = new FormatOptionsGoogleData(true).FormatOptions;
          SelectedOption = formatStyleOptions.First();
          isCustomStyle = false;
        }
      }
    }

    public string NameColumnWidth
    {
      get
      {
        return nameColumnWidth;
      }
      set
      {
        nameColumnWidth = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NameColumnWidth"));
      }
    }

    public string EnableOptionColumnWidth
    {
      get
      {
        return nameColumnWidth;
      }
      set
      {
        nameColumnWidth = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EnableOptionColumnWidth"));
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


    public ICommand SelctCodeFileCommand
    {
      get => selctCodeFileCommand ?? (selctCodeFileCommand = new RelayCommand(() => ReadCodeFromFile(), () => CanExecute));
    }

    #endregion


    #region Methods

    private void InitializeStyleOptions()
    {
      formatOptionsData = new FormatOptionsData(true);
      formatStyleOptions = formatOptionsData.FormatOptions;

      formatOptionsData.Initializing = false;
      formatOptionsData.DisableAllOptions();
      SelectedOption = formatStyleOptions.First();
    }

    private void OpenUri(string uri)
    {
      Process.Start(new ProcessStartInfo(uri));
    }

    private void ReadCodeFromFile()
    {
      var filePath = OpenFile(string.Empty, ".cpp", FileExtensionsSelectFile);

      if (File.Exists(filePath))
        formatOptionsView.CodeEditor.Text = File.ReadAllText(filePath);
    }

    private void ResetOptions()
    {
      InitializeStyleOptions();
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FormatOptions"));
    }

    private void CreateFormatFile()
    {
      string fileName = ".clang-format";
      string defaultExt = ".clang-format";
      string filter = "Configuration files (.clang-format)|*.clang-format";

      string path = SaveFile(fileName, defaultExt, filter);
      if (string.IsNullOrEmpty(path) == false)
      {
        WriteContentToFile(path, FormatOptionFile.CreateOutput(formatStyleOptions).ToString());
      }
    }

    public void RunFormat()
    {
      if (CheckIfAnyOptionEnabled() == false) return;
      if (formatOptionsData.Initializing) return;

      var text = formatOptionsView.CodeEditor.Text;
      string filePath = Path.Combine(settingsPathBuilder.GetPath(""), "FormatTemp.cpp");
      string formatFilePath = Path.Combine(settingsPathBuilder.GetPath(""), ".clang-format");

      WriteContentToFile(formatFilePath, FormatOptionFile.CreateOutput(formatStyleOptions).ToString());
      WriteContentToFile(filePath, text);

      var content = FormatFileOutsideProject(settingsPathBuilder.GetPath(""), filePath);
      formatOptionsView.CodeEditorReadOnly.Text = content;

      FileSystem.DeleteFile(filePath);
      FileSystem.DeleteFile(formatFilePath);
    }

    private static string FormatFileOutsideProject(string path, string filePath)
    {
      string vsixPath = Path.GetDirectoryName(
        typeof(RunClangPowerToolsPackage).Assembly.Location);

      var process = new Process();
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
      if (string.IsNullOrWhiteSpace(output)) output = process.StandardError.ReadToEnd();
      process.WaitForExit();
      process.Close();

      return output;
    }

    private bool CheckIfAnyOptionEnabled()
    {
      foreach (var item in formatStyleOptions)
      {
        if (item.IsEnabled == true) return true;
      }

      return false;
    }

    #endregion
  }
}
