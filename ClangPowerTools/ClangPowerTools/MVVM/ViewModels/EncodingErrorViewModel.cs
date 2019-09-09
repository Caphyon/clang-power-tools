using ClangPowerTools.MVVM.Commands;
using ClangPowerTools.MVVM.Models;
using ClangPowerTools.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace ClangPowerTools
{
  class EncodingErrorViewModel : INotifyPropertyChanged
  {
    #region Public Properties
    public ICommand CloseCommand { get; set; }
    public ICommand ConvertCommand { get; set; }
    public ICommand SearchCommand { get; set; }
    public Action CloseAction { get; set; }
    public ObservableCollection<FileModel> FilesNotEncodedInUTF8 { get; set; } = new ObservableCollection<FileModel>();

    public event PropertyChangedEventHandler PropertyChanged;

    public string SearchText
    {
      get { return searchText; }
      set
      {
        searchText = value;

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SearchText"));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FilteredFilesNotEncodedInUTF8"));
      }
    }

    public bool CheckAllItems
    {
      get { return checkAllItems; }
      set
      {
        if (checkAllItems == value) { return; }
        checkAllItems = value;
        SelectAllTooltipText = value ? Resources.DeselectAllTooltipText : Resources.SelectAllTooltipText;
        foreach (var file in FilesNotEncodedInUTF8)
        {
          file.IsChecked = value;
        }
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CheckAllItems"));
      }
    }

    public string SelectAllTooltipText
    {
      get { return selectAllTooltipText; }
      set
      {
        if (selectAllTooltipText == value) { return; }
        selectAllTooltipText = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectAllTooltipText"));
      }
    }
    public IEnumerable<FileModel> FilteredFilesNotEncodedInUTF8
    {
      get
      {
        if (SearchText == null)
        {
          return FilesNotEncodedInUTF8;
        }

        return FilesNotEncodedInUTF8.Where(x => x.FileName.ToUpper().Contains(SearchText.ToUpper()));
      }
    }

    #endregion

    #region Private Properties

    private string searchText;

    private readonly List<string> fileNames = new List<string>();

    private string selectAllTooltipText = Resources.DeselectAllTooltipText;

    private bool checkAllItems = true;
    #endregion

    #region Constructor

    public EncodingErrorViewModel(List<string> selectedDocuments)
    {
      fileNames = selectedDocuments;
      CloseCommand = new RelayCommand(CloseCommandExecute);
      ConvertCommand = new RelayCommand(ConvertCommandExecute);
      SearchCommand = new RelayCommand(SearchCommandExecute);
    }

    #endregion

    #region Public Methods
    public void LoadData()
    {
      foreach (var file in fileNames)
      {
        var encodingFile = GetEncoding(file);
        if (encodingFile.EncodingName != Encoding.UTF8.EncodingName && !file.EndsWith(".vcxproj") && !file.EndsWith(".sln"))
        {
          FilesNotEncodedInUTF8.Add(new FileModel { FileName = file, IsChecked = true });
        }
      }
    }

    #endregion

    #region Private Methods
    private void SearchCommandExecute()
    {
      if (false == string.IsNullOrWhiteSpace(SearchText))
      {
        SearchText = string.Empty;
      }
    }

    private void CloseCommandExecute()
    {
      CloseAction?.Invoke();
    }

    private void ConvertCommandExecute()
    {
      var checkedFiles = FilesNotEncodedInUTF8.Where(f => f.IsChecked);
      if (!checkedFiles.Any())
      {
        return;
      }
      foreach (var file in checkedFiles)
      {
          ConvertFileToUTF8(file.FileName);
      }
      CloseCommandExecute();
    }

    private void ConvertFileToUTF8(string file)
    {
      StreamReader streamReader = new StreamReader(file);
      string fileContent = streamReader.ReadToEnd();
      streamReader.Close();
      File.WriteAllText(file, fileContent, Encoding.UTF8);
    }

    private Encoding GetEncoding(string fileName)
    {
      using (var reader = new StreamReader(fileName, Encoding.Default, true))
      {
        if (reader.Peek() >= 0)
        {
          reader.Read();
        }
        return reader.CurrentEncoding;
      }
    }

    #endregion
  }
}
