using ClangPowerTools.Helpers;
using ClangPowerTools.MVVM.Commands;
using ClangPowerTools.MVVM.Interfaces;
using ClangPowerTools.MVVM.Views;
using ClangPowerTools.Views;
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
  public class FormatEditorViewModel : CommonSettingsFunctionality, INotifyPropertyChanged
  {
    #region Members

    public event PropertyChangedEventHandler PropertyChanged;

    private FormatOptionsData customOptionsData = new FormatOptionsData();
    private FormatOptionsData llvmOptionsData = new FormatOptionsData();
    private FormatOptionsGoogleData googleOptionsData = new FormatOptionsGoogleData();
    private FormatOptionsChromiumData chromiumOptionsData = new FormatOptionsChromiumData();
    private FormatOptionsMozillaData mozillaOptionsData = new FormatOptionsMozillaData();
    private FormatOptionsWebKitData webkitOptionsData = new FormatOptionsWebKitData();
    private FormatOptionsMicrosoftData microsoftOptionsData = new FormatOptionsMicrosoftData();

    private readonly SettingsPathBuilder settingsPathBuilder;
    private readonly FormatEditorView formatOptionsView;
    private ICommand selctCodeFileCommand;
    private ICommand createFormatFileCommand;
    private ICommand formatCodeCommand;
    private ICommand resetCommand;
    private ICommand openUri;
    private ICommand openMultipleInputCommand;
    
    private IFormatOption selectedOption;
    private List<IFormatOption> formatStyleOptions;
    private EditorStyles editorStyle = EditorStyles.Custom;
    private string nameColumnWidth;
    private const string autoSize = "auto";
    private const string nameColumnWidthMax = "340";
    public const string FileExtensionsSelectFile = "Code files (*.c;*.cpp;*.cxx;*.cc;*.tli;*.tlh;*.h;*.hh;*.hpp;*.hxx;*.h;*.h;*.h)|*.c;*.cpp;*.cxx;*.cc;*.tli;*.tlh;*.h;*.hh;*.hpp;*.hxx";
    #endregion

    #region Constructor

    public FormatEditorViewModel(FormatEditorView formatOptionsView)
    {
      settingsPathBuilder = new SettingsPathBuilder();
      this.formatOptionsView = formatOptionsView;
      InitializeStyleOptions(customOptionsData);
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
        return editorStyle;
      }
      set
      {
        editorStyle = value;
        switch (editorStyle)
        {
          case EditorStyles.Custom:
            SetStyleControls(autoSize, autoSize, customOptionsData.FormatOptions);
            break;
          case EditorStyles.LLVM:
            SetStyleControls(nameColumnWidthMax, "0", llvmOptionsData.FormatOptions);
            break;
          case EditorStyles.Google:
            SetStyleControls(nameColumnWidthMax, "0", googleOptionsData.FormatOptions);
            break;
          case EditorStyles.Chromium:
            SetStyleControls(nameColumnWidthMax, "0", chromiumOptionsData.FormatOptions);
            break;
          case EditorStyles.Mozilla:
            SetStyleControls(nameColumnWidthMax, "0", mozillaOptionsData.FormatOptions);
            break;
          case EditorStyles.WebKit:
            SetStyleControls(nameColumnWidthMax, "0", webkitOptionsData.FormatOptions);
            break;
          case EditorStyles.Microsoft:
            SetStyleControls(nameColumnWidthMax, "0", microsoftOptionsData.FormatOptions);
            break;
        }

        SelectedOption = formatStyleOptions.First();
        RunFormat();
      }
    }

    private void SetStyleControls(string nameColumnWidth, string enableOptionColumnWidth, List<IFormatOption> options)
    {
      NameColumnWidth = nameColumnWidth;
      EnableOptionColumnWidth = enableOptionColumnWidth;
      FormatOptions = options;
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

    public ICommand OpenMultipleInputCommand
    {
      get => openMultipleInputCommand ?? (openMultipleInputCommand = new RelayCommand(() => OpenInputDataView(), () => CanExecute));
    }

    #endregion


    #region Methods

    private void InitializeStyleOptions(FormatOptionsData formatOptionsData)
    {
      formatStyleOptions = formatOptionsData.FormatOptions;
      formatOptionsData.DisableAllOptions();
      SelectedOption = formatStyleOptions.First();
    }

    private void OpenInputDataView()
    {
      InputMultipleDataView inputMultipleDataView = new InputMultipleDataView();
      inputMultipleDataView.Show();
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
      customOptionsData = new FormatOptionsData();
      llvmOptionsData = new FormatOptionsData();
      googleOptionsData = new FormatOptionsGoogleData();
      chromiumOptionsData = new FormatOptionsChromiumData();
      mozillaOptionsData = new FormatOptionsMozillaData();
      webkitOptionsData = new FormatOptionsWebKitData();
      microsoftOptionsData = new FormatOptionsMicrosoftData();

      InitializeStyleOptions(customOptionsData);
      SelectedStyle = EditorStyles.Custom;

      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedStyle"));
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
        WriteContentToFile(path, FormatOptionFile.CreateOutput(formatStyleOptions, SelectedStyle).ToString());
      }
    }

    public void RunFormat()
    {
      if (CheckIfAnyOptionEnabled() == false) return;

      var text = formatOptionsView.CodeEditor.Text;
      string filePath = Path.Combine(settingsPathBuilder.GetPath(""), "FormatTemp.cpp");
      string formatFilePath = Path.Combine(settingsPathBuilder.GetPath(""), ".clang-format");

      WriteContentToFile(formatFilePath, FormatOptionFile.CreateOutput(formatStyleOptions, SelectedStyle).ToString());
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
