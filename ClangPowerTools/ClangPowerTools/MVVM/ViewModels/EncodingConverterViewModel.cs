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
    //public ObservableCollection<FileModel> NonUTF8Files { get; set; } = new ObservableCollection<FileModel>();

    public ICommand CancelCommand { get; set; }
    public ICommand ConvertCommand { get; set; }
    public Action CloseAction { get; set; }

    private ObservableCollection<FileModel> filesNotEncodedInUTF8;
    private void OnPropertyChanged(string prop)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
    public ObservableCollection<FileModel> FilesNotEncodedInUTF8
    {
      get { return filesNotEncodedInUTF8; }
      set
      {
        if (filesNotEncodedInUTF8 == value) { return; }
        filesNotEncodedInUTF8 = value;
        OnPropertyChanged("FilesNotEncodedInUTF8");
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    //[NotifyPropertyChangedInvocator]
    //protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    //{
    //  PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    //}


    private readonly List<string> fileNames = new List<string>();

    private bool isConvertButtonEnabled = true;

    public bool IsConvertButtonEnabled
    {
      get { return isConvertButtonEnabled; }
      set
      {
        if (isConvertButtonEnabled == value) return;
        isConvertButtonEnabled = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsConvertButtonEnabled"));
      }
    }

    public EncodingConverterViewModel(List<string> selectedDocuments)
    {
      fileNames = selectedDocuments;
      CancelCommand = new RelayCommand(CancelCommandExecute);
      ConvertCommand = new RelayCommand(ConvertCommandExecute);
      EventBus.Register("IsConvertButtonEnable", IsConvertButtonEnabledExecute);
      FilesNotEncodedInUTF8 = new ObservableCollection<FileModel>();
      //FilesNotEncodedInUTF8.CollectionChanged += CheckedUtf8FilesChanged;
    }

    private void IsConvertButtonEnabledExecute()
    {
      IsConvertButtonEnabled = filesNotEncodedInUTF8.Where(f=>f.IsChecked).Any();
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

    private void CheckedUtf8FilesChanged(object sender, NotifyCollectionChangedEventArgs e)
    {

    }

    private void CancelCommandExecute()
    {
      EventBus.Unregister("IsConvertButtonEnable", IsConvertButtonEnabledExecute);
      CloseAction?.Invoke();
    }

    private void ConvertCommandExecute()
    {
      foreach (var file in FilesNotEncodedInUTF8)
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
