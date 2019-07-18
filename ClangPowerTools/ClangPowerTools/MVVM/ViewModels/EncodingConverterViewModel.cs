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
    public ICommand CancelCommand { get; set; }
    public ICommand ConvertCommand { get; set; }

    public ICommand SelectAllCommand { get; set; }

    public string SelectAllButtonContent
    {
      get { return selectAllButtonContent; }
      set
      {
        if (selectAllButtonContent == value) { return; }
        selectAllButtonContent = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectAllButtonContent"));
      }
    }

    public Action CloseAction { get; set; }

    public ObservableCollection<FileModel> FilesNotEncodedInUTF8 { get; set; } = new ObservableCollection<FileModel>();

    public event PropertyChangedEventHandler PropertyChanged;

    private readonly List<string> fileNames = new List<string>();

    private bool isConvertButtonEnabled = true;

    private string selectAllButtonContent = Resources.DeselectAllButtonText;

    public bool IsConvertButtonEnabled
    {
      get { return isConvertButtonEnabled; }
      set
      {
        if(isConvertButtonEnabled == value) { return; }
        isConvertButtonEnabled = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsConvertButtonEnabled"));
      }
    }

    public EncodingConverterViewModel(List<string> selectedDocuments)
    {
      fileNames = selectedDocuments;
      CancelCommand = new RelayCommand(CancelCommandExecute);
      ConvertCommand = new RelayCommand(ConvertCommandExecute);
      SelectAllCommand = new RelayCommand(SelectAllCommandExecute);
      EventBus.Register("IsConvertButtonEnable", IsConvertButtonEnabledExecute);
    }

    private void SelectAllCommandExecute()
    {
      if (SelectAllButtonContent == Resources.SelectAllButtonText)
      {
        SelectAllButtonContent = Resources.DeselectAllButtonText;
        foreach (var file in FilesNotEncodedInUTF8)
        {
          file.IsChecked = true;
        }
      }
      else
      {
        SelectAllButtonContent = Resources.SelectAllButtonText;
        foreach (var file in FilesNotEncodedInUTF8)
        {
          file.IsChecked = false;
        }
      }
    }

    private void IsConvertButtonEnabledExecute()
    {
      if (!FilesNotEncodedInUTF8.Any())
      {
        return;
      }
      IsConvertButtonEnabled = FilesNotEncodedInUTF8.Where(f => f.IsChecked).Any();
      if(!isConvertButtonEnabled)
      {
        SelectAllButtonContent = Resources.SelectAllButtonText;
      }
      else if(!FilesNotEncodedInUTF8.Where(f => !f.IsChecked).Any())
      {
        SelectAllButtonContent = Resources.DeselectAllButtonText;
      }
     
    }

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

    private void CancelCommandExecute()
    {
      EventBus.Unregister("IsConvertButtonEnable", IsConvertButtonEnabledExecute);
      CloseAction?.Invoke();
    }

    private void ConvertCommandExecute()
    {
      var checkedFiles = FilesNotEncodedInUTF8.Where(f => f.IsChecked);
      foreach (var file in checkedFiles)
      {
        if (file.IsChecked)
        {
          ConvertFileToUTF8(file.FileName);
        }
      }
      CancelCommandExecute();
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
