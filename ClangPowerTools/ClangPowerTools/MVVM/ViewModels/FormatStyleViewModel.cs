using ClangPowerTools.MVVM.Commands;
using ClangPowerTools.MVVM.Constants;
using ClangPowerTools.MVVM.Interfaces;
using ClangPowerTools.MVVM.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Process = System.Diagnostics.Process;

namespace ClangPowerTools
{
  public class FormatStyleViewModel : CommonSettingsFunctionality, INotifyPropertyChanged
  {
    #region Members

    public event PropertyChangedEventHandler PropertyChanged;

    private readonly FormatOptionsView formatOptionsView;
    private ICommand createFormatFileCommand;
    private ICommand formatCodeCommand;
    private ICommand openUri;
    private IFormatOption selectedOption;
    private readonly SettingsPathBuilder settingsPathBuilder = new SettingsPathBuilder();
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
        return FormatOptionsData.FormatOptions;
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

    #endregion


    #region Methods


    private void OpenUri(string uri)
    {
      Process.Start(new ProcessStartInfo(uri));
    }

    private void CreateFormatFile()
    {
      string fileName = ".clang-format";
      string defaultExt = ".clang-format";
      string filter = "Configuration files (.clang-format)|*.clang-format";

      string path = SaveFile(fileName, defaultExt, filter);
      if (string.IsNullOrEmpty(path) == false)
      {
        WriteContentToFile(path, FormatOptionFile.CreateOutput().ToString());
      }
    }

    private void RunFormat()
    {
      var document = formatOptionsView.CodeEditor.Document;
      var text = new TextRange(document.ContentStart, document.ContentEnd).Text;
      string filePath = Path.Combine(settingsPathBuilder.GetPath(""), "FormatTemp.cpp");
      string formatFilePath = Path.Combine(settingsPathBuilder.GetPath(""), ".clang-format");

      if (CheckOptionsEnabled()) return;
      WriteContentToFile(formatFilePath, FormatOptionFile.CreateOutput().ToString());
      CreateTempCppFile(text, filePath);

      var content = FormatFileOutsideProject(settingsPathBuilder.GetPath(""), filePath);
      TextManipulation.ReplaceAllTextInFlowDocument(formatOptionsView.CodeEditor.Document, content);

      DeleteFile(filePath);
      //DeleteFile(formatFilePath);
    }

    private bool CheckOptionsEnabled()
    {
      foreach (var item in FormatOptionsData.FormatOptions)
      {
        if (item.IsEnabled) return true;
      }
      return false;
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

    public void HighlightText()
    {
      var document = formatOptionsView.CodeEditor.Document;
      TextManipulation.HighlightKeywords(document.ContentStart, document.ContentEnd, CPPKeywords.keywords, new SolidColorBrush(Color.FromRgb(239, 153, 142)));
    }

    public void RestCodeEditorFormat()
    {
      formatOptionsView.CodeEditor.SelectAll();
      formatOptionsView.CodeEditor.Selection.ClearAllProperties();
      formatOptionsView.CodeEditor.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, 14.0);
      formatOptionsView.CodeEditor.Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, "Courier New");
    }

    private void CreateTempCppFile(string content, string filePath)
    {
      using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate))
      {
        using StreamWriter sw = new StreamWriter(fs);
        sw.Write(content);
      }
    }

    private void DeleteFile(string path)
    {
      if (File.Exists(path))
      {
        File.Delete(path);
      }
    }


    #endregion
  }
}
