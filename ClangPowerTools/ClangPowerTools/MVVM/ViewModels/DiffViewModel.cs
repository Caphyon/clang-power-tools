using ClangPowerTools.MVVM.Commands;
using ClangPowerTools.MVVM.Views;
using System;
using System.Text;
using System.Windows.Documents;
using System.Windows.Input;

namespace ClangPowerTools
{
  public class DiffViewModel
  {
    #region Members

    private ICommand createFormatFileCommand;
    private readonly Action CreateFormatFile;
    private const int PageWith = 1000;

    #endregion


    #region Properties

    public string FormatOptionFile { get; set; }

    public bool CanExecute
    {
      get
      {
        return true;
      }
    }

    #endregion

    #region Constructor 

    public DiffViewModel(DiffWindow diffWindow, FlowDocument diffInput, FlowDocument diffOutput, string formatOptionFile, Action CreateFormatFile)
    {
      diffInput.PageWidth = PageWith;
      diffOutput.PageWidth = PageWith;
      diffWindow.DiffInput.Document = diffInput;
      diffWindow.DiffOutput.Document = diffOutput;
      FormatOptionFile = CleanOptionFile(formatOptionFile);
      this.CreateFormatFile = CreateFormatFile;
    }

    //Empty constructor used for XAML IntelliSense
    public DiffViewModel()
    {

    }

    #endregion

    #region Commands

    public ICommand CreateFormatFileCommand
    {
      get => createFormatFileCommand ??= new RelayCommand(() => CreateFormatFile.Invoke(), () => CanExecute);
    }

    #endregion

    #region Methods

    private string CleanOptionFile(string formatOptionFile)
    {
      var lines = formatOptionFile.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
      var sb = new StringBuilder();
      for (int i = 2; i < lines.Length - 1; i++)
      {
        sb.AppendLine(lines[i]);
      }
      return sb.ToString();
    }

    #endregion

  }
}
