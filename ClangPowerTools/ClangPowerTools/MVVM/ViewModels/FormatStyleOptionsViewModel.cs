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


    public void HighlightText()
    {
      var document = formatOptionsView.CodeEditor.Document;

      foreach (var keyword in CPPKeywords.keywords)
      {
        TextManipulation.FromTextPointer(document.ContentStart, document.ContentEnd, keyword, Brushes.Red);
      }
      TextRange tr = new TextRange(document.ContentEnd, document.ContentEnd);
      tr.Text = "X";
      tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
    }
    #endregion
  }
}
