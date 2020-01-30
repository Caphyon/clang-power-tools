using ClangPowerTools.MVVM.Commands;
using ClangPowerTools.MVVM.Constants;
using ClangPowerTools.MVVM.Interfaces;
using ClangPowerTools.MVVM.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace ClangPowerTools
{
  public class FormatStyleOptionsViewModel : CommonSettingsFunctionality, INotifyPropertyChanged
  {
    #region Members

    public event PropertyChangedEventHandler PropertyChanged;

    private FormatOptionsView formatOptionsView;
    private ICommand createFormatFileCommand;
    private IFormatOption selectedOption;
    private string codeEditorText;

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

    public string CodeEditorText
    {
      get
      {
        return codeEditorText;
      }
      set
      {
        codeEditorText = value;
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


    private void HighlightText()
    {
      if (string.IsNullOrEmpty(codeEditorText)) return;

      var index = codeEditorText.LastIndexOf(' ');
      var word = codeEditorText.Substring(index, codeEditorText.Length - 1);

      if (CPPKeywords.keywords.Contains(word))
      {
        Run run = new Run(word);
        run.Foreground = Brushes.Red;
        TextRange textRange = new TextRange(formatOptionsView.CodeEditor.Document.ContentEnd, formatOptionsView.CodeEditor.Document.ContentEnd);

      }
      else
      {

      }
    }


    #endregion
  }
}
