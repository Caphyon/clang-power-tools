using ClangPowerTools.MVVM.Commands;
using ClangPowerTools.MVVM.Constants;
using ClangPowerTools.MVVM.Interfaces;
using ClangPowerTools.MVVM.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Process = System.Diagnostics.Process;

namespace ClangPowerTools
{
  public class FormatStyleOptionsViewModel : CommonSettingsFunctionality, INotifyPropertyChanged
  {
    #region Members

    public event PropertyChangedEventHandler PropertyChanged;

    private FormatOptionsView formatOptionsView;
    private ICommand createFormatFileCommand;
    private ICommand formatCodeCommand;
    private IFormatOption selectedOption;
    private readonly SettingsPathBuilder settingsPathBuilder = new SettingsPathBuilder();
    #endregion

    #region Constructor

    public FormatStyleOptionsViewModel(FormatOptionsView formatOptionsView)
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

    #endregion


    #region Methods



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

      var tempFile = CreateTempCppFile(text);
      FormatFileOutsideProject(settingsPathBuilder.GetPath(""), tempFile);
      DeleteTempFile(tempFile);
    }

    public static void FormatFileOutsideProject(string path, string filePath)
    {
      string vsixPath = Path.GetDirectoryName(
        typeof(RunClangPowerToolsPackage).Assembly.Location);

      Process process = new Process();
      process.StartInfo.UseShellExecute = false;
      process.StartInfo.CreateNoWindow = true;
      process.StartInfo.RedirectStandardInput = true;
      process.StartInfo.RedirectStandardOutput = true;
      process.StartInfo.RedirectStandardError = true;
      process.StartInfo.FileName = Path.Combine(vsixPath, ScriptConstants.kClangFormat);
      process.StartInfo.WorkingDirectory = path;
      process.StartInfo.Arguments = $"-i \"{Path.GetFullPath(filePath)}\"";

      process.Start();
      process.WaitForExit();
    }



    public void HighlightText()
    {
      var document = formatOptionsView.CodeEditor.Document;

      foreach (var keyword in CPPKeywords.keywords)
      {
        TextManipulation.FromTextPointer(document.ContentStart, document.ContentEnd, keyword, Brushes.Red);
      }
      //TextRange tr = new TextRange(document.ContentEnd, document.ContentEnd);
      //tr.Text = "X";
      //tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
    }



    private string CreateTempCppFile(string content)
    {
      string tempFile = Path.Combine(settingsPathBuilder.GetPath(""), "FormatTemp.cpp");

      using (FileStream fs = new FileStream(tempFile, FileMode.OpenOrCreate))
      {
        using StreamWriter sw = new StreamWriter(fs);
        sw.Write(content);
      }

      return tempFile;
    }

    private void DeleteTempFile(string tempFile)
    {
      File.Delete(tempFile);
    }

    #endregion
  }
}
