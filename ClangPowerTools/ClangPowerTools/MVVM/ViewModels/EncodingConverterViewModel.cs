using ClangPowerTools.MVVM.Command;
using ClangPowerTools.MVVM.Models;
using ClangPowerTools.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClangPowerTools.MVVM.ViewModels
{
  class EncodingConverterViewModel : INotifyPropertyChanged
  {
    private readonly List<string> fileNames = new List<string>();
    private HashSet<Encoding> fileEncodings = new HashSet<Encoding>();
    private EncodingModel _selectedEncoding;
    private bool _isCheckBoxListVisible = true;

    public string CurrentEncodingText { get; set; }
    public ObservableCollection<FileModel> NonUTF8Files { get; set; } = new ObservableCollection<FileModel>();
    public ObservableCollection<EncodingModel> EncodingCollection { get; set; }
    public ICommand CancelCommand { get; set; }
    public ICommand ConvertCommand { get; set; }

    public EncodingModel SelectedEncoding
    {
      get { return _selectedEncoding; }
      set
      {
        if (_selectedEncoding == value) return;
        _selectedEncoding = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedEncoding"));
      }
    }
    public bool IsCheckBoxListVisible
    {
      get { return _isCheckBoxListVisible; }
      set
      {
        if (_isCheckBoxListVisible == value) return;
        _isCheckBoxListVisible = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsCheckBoxListVisible"));
      }
    }
    public Action CloseAction { get; set; }


    public event PropertyChangedEventHandler PropertyChanged;

    public EncodingConverterViewModel(List<string> selectedDocuments)
    {
      fileNames = selectedDocuments;
    }

    public void LoadData()
    {
      CancelCommand = new RelayCommand(CancelCommandExecute);

      ConvertCommand = new RelayCommand(ConvertCommandExecute);

      foreach (var file in fileNames)
      {
        var encodingFile = GetEncoding(file);
        if (encodingFile.EncodingName != Encoding.UTF8.EncodingName && !file.EndsWith(".vcxproj") && !file.EndsWith(".sln"))
        {
          NonUTF8Files.Add(new FileModel { FileName = file, IsChecked = true });
        }
        fileEncodings.Add(encodingFile);
      }
      if(!NonUTF8Files.Any())
      {
          IsCheckBoxListVisible = false;
      }

      if (!fileEncodings.Any())
      {
        CurrentEncodingText = string.Format("Current Encoding: {0}", Resources.NoEncodingDetected);
      }
      else if (fileEncodings.Count() == 1)
      {
        CurrentEncodingText = string.Format("Current Encoding: {0}", fileEncodings.First().EncodingName);
      }
      else
      {
        CurrentEncodingText = string.Format("Current Encoding: {0}", Resources.MultipleEncodingsSelected);
      }

      InitializeEncodingList();
    }

    private void InitializeEncodingList()
    {
      EncodingCollection = new ObservableCollection<EncodingModel>();

      AddEncoding(Encoding.UTF8, "UTF-8");
      AddEncoding(Encoding.UTF32, "UTF-32");
      AddEncoding(Encoding.Unicode, "Unicode");
      AddEncoding(Encoding.BigEndianUnicode, "Big Endian");
      //AddEncoding(Encoding.GetEncoding("utf-32BE"), "UTF-32 Big Endian");

      SelectedEncoding = EncodingCollection.FirstOrDefault(e => e.Encoding.EncodingName == Resources.UTF8Encoding);
    }

    private void AddEncoding(Encoding encoding, string encodingName)
    {
      EncodingCollection.Add(new EncodingModel { EncodingName = encodingName, Encoding = encoding });
    }

    private void CancelCommandExecute()
    {
      CloseAction?.Invoke();
    }

    private void ConvertCommandExecute()
    {
      foreach (var file in NonUTF8Files)
      {
        if (file.IsChecked)
        {
          ConvertFile(file.FileName);
        }
      }

      CancelCommandExecute();
    }

    private void ConvertFile(string file)
    {
      if (SelectedEncoding == null || fileEncodings.Count() == 1 && SelectedEncoding.Encoding == fileEncodings.First())
      {
        return;
      }

      StreamReader streamReader = new StreamReader(file);
      string fileContent = streamReader.ReadToEnd();
      streamReader.Close();
      File.WriteAllText(file, fileContent, SelectedEncoding.Encoding);
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
