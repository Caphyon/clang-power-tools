using ClangPowerTools.MVVM.Commands;
using ClangPowerTools.MVVM.Models;
using ClangPowerTools.MVVM.Views;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClangPowerTools
{
  public class DetectStyleFileSelectorViewModel : CommonSettingsFunctionality
  {
    #region Members

    private readonly DetectStyleFileSelectorView view;

    private ICommand browseCommand;
    private ICommand detectFormatStyleCommand;

    long totalFilesSize = 0;
    private const long MAX_FILE_SIZE = 1500000; // 1.5 MB

    #endregion


    #region Constructor

    public DetectStyleFileSelectorViewModel() { }

    public DetectStyleFileSelectorViewModel(DetectStyleFileSelectorView view)
    {
      this.view = view;
      view.DetectFormatStyleButton.IsEnabled = false;
    }

    #endregion


    #region Properties

    public ObservableCollection<SelectedFileModel> SelectedFiles { get; set; } = new ObservableCollection<SelectedFileModel>();

    public ICommand Browse
    {
      get => browseCommand ?? (browseCommand = new RelayCommand(() => BrowseForFiles(), () => CanExecute));
    }

    public ICommand DetectFormatStyleCommand
    {
      get => detectFormatStyleCommand ?? (detectFormatStyleCommand = new RelayCommand(() => DetectFormatStyleAsync().SafeFireAndForget(), () => CanExecute));
    }

    public bool CanExecute
    {
      get
      {
        return true;
      }
    }

    #endregion


    #region Methods

    private void BrowseForFiles()
    {
      string[] filePaths = OpenFiles(string.Empty, ".cpp", ScriptConstants.FileExtensionsSelectFile);

      if (filePaths == null || filePaths.Length <= 0)
        return;

      for (int index = 0; index < filePaths.Length; ++index)
      {
        if (IsDuplicate(filePaths[index]))
          continue;

        AddNewElement(filePaths[index]);
      }

      view.DetectFormatStyleButton.IsEnabled = SelectedFiles.Count > 0;

      if (totalFilesSize > MAX_FILE_SIZE &&
        view.WarningTextBox.Visibility != System.Windows.Visibility.Visible)
      {
        view.WarningTextBox.Visibility = System.Windows.Visibility.Visible;
        var warning = new FileSizeWarningView(view);
        warning.Show();
      }
    }

    private bool IsDuplicate(string filePath) => SelectedFiles.FirstOrDefault(model => model.FilePath == filePath) != null;

    private void AddNewElement(string filePath)
    {
      var model = new SelectedFileModel(filePath);
      totalFilesSize += model.FileSize;
      SelectedFiles.Add(model);
    }

    public void RemoveFile(int index)
    {
      if (index < 0 || index >= SelectedFiles.Count)
        return;

      SelectedFileModel model = SelectedFiles[index];
      totalFilesSize -= model.FileSize;
      SelectedFiles.RemoveAt(index);

      view.DetectFormatStyleButton.IsEnabled = SelectedFiles.Count != 0;

      if (totalFilesSize <= MAX_FILE_SIZE)
        view.WarningTextBox.Visibility = System.Windows.Visibility.Hidden;
    }

    private async Task DetectFormatStyleAsync()
    {
      var diffWin = new DiffWindow();
      List<string> filesPath = SelectedFiles.Select(model => model.FilePath).ToList();

      view.IsEnabled = false;
      await diffWin.ShowDiffAsync(filesPath, view);
      view.IsEnabled = true;
    }

    #endregion

  }
}
