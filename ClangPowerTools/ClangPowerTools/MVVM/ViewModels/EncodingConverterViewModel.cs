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
    private readonly List<IItem> fileNames = new List<IItem>();
    private HashSet<Encoding> fileEncodings = new HashSet<Encoding>();
    private EncodingModel _selectedEncoding;

    public string CurrentEncodingText { get; set; }
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
    public Action CloseAction { get; set; }


    public event PropertyChangedEventHandler PropertyChanged;

    public EncodingConverterViewModel(List<IItem> selectedDocuments)
    {
      fileNames = selectedDocuments;
    }

    public async Task LoadData()
    {
      CancelCommand = new RelayCommand(o => { CancelCommandExecute(); }, o => true);

      ConvertCommand = new RelayCommand(o => { ConvertCommandExecute(); }, o => true);

      foreach (var file in fileNames)
      {
        fileEncodings.Add(GetEncoding(file.GetPath()));
      }

      CurrentEncodingText = string.Format("Current Encoding: {0}", fileEncodings.Count() == 1 ? fileEncodings.First().EncodingName : "multiple encodings");

      InitializeEncodingList();
    }

    private void InitializeEncodingList()
    {
      EncodingCollection = new ObservableCollection<EncodingModel>();

      AddEncoding(Encoding.UTF8, "UTF-8");
      AddEncoding(Encoding.UTF32, "UTF-32");
      AddEncoding(Encoding.Unicode, "Unicode");
      AddEncoding(Encoding.BigEndianUnicode, "Big Endian");
      AddEncoding(Encoding.GetEncoding("utf-32BE"), "UTF-32 Big Endian");

      //foreach (EncodingInfo ei in Encoding.GetEncodings())
      //{
      //    AddEncoding(ei.GetEncoding());
      //}

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
      foreach (var file in fileNames)
      {
        ConvertFile(file.GetPath());
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
