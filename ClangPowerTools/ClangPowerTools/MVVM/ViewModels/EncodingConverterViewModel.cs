using ClangPowerTools.MVVM.Command;
using ClangPowerTools.MVVM.Models;
using ClangPowerTools.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ClangPowerTools.MVVM.Utils;

namespace ClangPowerTools.MVVM.ViewModels
{
  class EncodingConverterViewModel : INotifyPropertyChanged
  {
    public ICommand CloseCommand { get; set; }
    public ICommand ConvertCommand { get; set; }

    public ICommand SearchCommand { get; set; }

    //public ICommand SelectAllCommand { get; set; }

    private string searchText;

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
        foreach (var file in FilesNotEncodedInUTF8)
        {
          file.IsChecked = value;
        }
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CheckAllItems"));
      }
    }

    //public string SelectAllButtonContent
    //{
    //  get { return selectAllButtonContent; }
    //  set
    //  {
    //    if (selectAllButtonContent == value) { return; }
    //    selectAllButtonContent = value;
    //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectAllButtonContent"));
    //  }
    //}

    public Action CloseAction { get; set; }

    public ObservableCollection<FileModel> FilesNotEncodedInUTF8 { get; set; } = new ObservableCollection<FileModel>();
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


    public event PropertyChangedEventHandler PropertyChanged;

    private readonly List<string> fileNames = new List<string>();

    private bool isConvertButtonEnabled = true;

    //private string selectAllButtonContent = Resources.DeselectAllButtonText;

    private bool checkAllItems = true;

    public bool IsConvertButtonEnabled
    {
      get { return isConvertButtonEnabled; }
      set
      {
        if (isConvertButtonEnabled == value) { return; }
        isConvertButtonEnabled = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsConvertButtonEnabled"));
      }
    }

    public EncodingConverterViewModel(List<string> selectedDocuments)
    {
      fileNames = selectedDocuments;
      CloseCommand = new RelayCommand(CloseCommandExecute);
      ConvertCommand = new RelayCommand(ConvertCommandExecute);
      SearchCommand = new RelayCommand(SearchCommandExecute);
      //SelectAllCommand = new RelayCommand(SelectAllCommandExecute);
      //EventBus.Register("EnableConvertButtonEvent", EnableConvertButtonCallback);
      //EventBus.Register("DisableConvertButtonEvent", DisableConvertButtonCallback);

    }

    private void SearchCommandExecute()
    {
      if (false == string.IsNullOrWhiteSpace(SearchText))
      {
        SearchText = string.Empty;
      }
      // Put the mouse cursor inside the SearchBox by focus it
      //SearchText.Focus();
    }

    //private void DisableConvertButtonCallback()
    //{
    //  IsConvertButtonEnabled = false;
    //  SelectAllButtonContent = Resources.SelectAllButtonText;
    //}

    //private void EnableConvertButtonCallback()
    //{
    //  IsConvertButtonEnabled = true;
    //  SelectAllButtonContent = Resources.DeselectAllButtonText;
    //}

    //private void SelectAllCommandExecute()
    //{
    //  if (SelectAllButtonContent == Resources.SelectAllButtonText)
    //  {
    //    SelectAllButtonContent = Resources.DeselectAllButtonText;
    //    foreach (var file in FilesNotEncodedInUTF8)
    //    {
    //      file.IsChecked = true;
    //    }
    //  }
    //  else
    //  {
    //    SelectAllButtonContent = Resources.SelectAllButtonText;
    //    foreach (var file in FilesNotEncodedInUTF8)
    //    {
    //      file.IsChecked = false;
    //    }
    //  }
    //}


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

    private void CloseCommandExecute()
    {
      //EventBus.Unregister("EnableConvertButtonEvent", EnableConvertButtonCallback);
      //EventBus.Unregister("DisableConvertButtonEvent", DisableConvertButtonCallback);
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
        if (file.IsChecked)
        {
          ConvertFileToUTF8(file.FileName);
        }
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
  }
}
