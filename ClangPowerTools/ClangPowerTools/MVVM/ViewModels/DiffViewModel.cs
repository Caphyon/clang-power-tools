using ClangPowerTools.MVVM.Commands;
using ClangPowerTools.MVVM.Views;
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace ClangPowerTools
{
  public class DiffViewModel : INotifyPropertyChanged
  {
    #region Members

    private ICommand createFormatFileCommand;
    private Action CreateFormatFile;

    public event PropertyChangedEventHandler PropertyChanged;

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

    public DiffViewModel(DiffWindow diffWindow, string html, string formatOptionFile, Action CreateFormatFile)
    {
      diffWindow.MyWebBrowser.NavigateToString(html);
      FormatOptionFile = formatOptionFile;
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

  }
}
