using ClangPowerTools.MVVM.Commands;
using ClangPowerTools.MVVM.Views;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace ClangPowerTools
{
  public class DetectFormatStyleMenuViewModel : CommonSettingsFunctionality
  {
    #region Members

    private DetectFormatStyleMenuView detectFormatStyleMenuView;
    private readonly FormatEditorViewModel formatEditorViewModel;
    private ICommand detectFormatOnCodeInputCommand;
    private ICommand detectFormatOnProjectFilesCommand;

    #endregion


    #region Constructor

    public DetectFormatStyleMenuViewModel() { }

    public DetectFormatStyleMenuViewModel(DetectFormatStyleMenuView view, FormatEditorViewModel formatEditorViewModel)
    {
      detectFormatStyleMenuView = view;
      this.formatEditorViewModel = formatEditorViewModel;
    }

    #endregion


    #region Properties

    public bool CanExecute
    {
      get
      {
        return true;
      }
    }

    #endregion


    #region Commands

    public ICommand DetectFormatOnCodeInput
    {
      get => detectFormatOnCodeInputCommand ??= new RelayCommand(() => DetectFormatOnCodeInputExecute(), () => CanExecute);
    }

    public ICommand DetectFormatOnProjectFiles
    {
      get => detectFormatOnProjectFilesCommand ??= new RelayCommand(() => DetectFormatOnProjectFilesExecute(), () => CanExecute);
    }

    #endregion


    #region Methods

    private void DetectFormatOnCodeInputExecute()
    {
      formatEditorViewModel.DetectStyleAsync(new List<string>()).SafeFireAndForget();
      detectFormatStyleMenuView.Close();
    }

    private void DetectFormatOnProjectFilesExecute()
    {
      string[] files = BrowseForFolderFiles("*.cpp", SearchOption.AllDirectories);

      if (files == null)
        return;

      formatEditorViewModel.DetectStyleAsync(files.ToList()).SafeFireAndForget();
      detectFormatStyleMenuView.Close();
    }

    #endregion

  }
}