using ClangPowerTools.MVVM.Commands;
using ClangPowerTools.MVVM.Interfaces;
using ClangPowerTools.MVVM.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace ClangPowerTools
{
  public class FormatStyleOptionsViewModel : CommonSettingsFunctionality, INotifyPropertyChanged
  {
    #region Members

    public event PropertyChangedEventHandler PropertyChanged;

    private ICommand createFormatFileCommand;
    private IFormatOption _selectedOption;

    #endregion

    #region Constructor

    public FormatStyleOptionsViewModel()
    {
      _selectedOption = FormatOptions.First();
    }

    #endregion

    #region Properties

    public List<IFormatOption> FormatOptions
    {
      get
      {
        return FormatOptionsData.FormatOptions;
      }
    }

    public IFormatOption SelectedOption
    {
      get
      {
        return _selectedOption;
      }
      set
      {
        _selectedOption = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedOption"));
      }
    }

    public IEnumerable<ClangFormatStyle> PredefinedStyles
    {
      get
      {
        return Enum.GetValues(typeof(ClangFormatStyle)).Cast<ClangFormatStyle>();
      }
    }

    public ClangFormatStyle SelectedPredefinedStyle { get; set; } = ClangFormatStyle.file;

    public bool CanExecute
    {
      get
      {
        return true;
      }
    }

    #endregion


    #region Commands

    public ICommand CreateFormatFileCommand
    {
      get => createFormatFileCommand ?? (createFormatFileCommand = new RelayCommand(() => CreateFormatFile(), () => CanExecute));
    }

    #endregion


    #region Methods

    private void CreateFormatFile()
    {
      string fileName = ".clang-format";
      string defaultExt = ".clang-format";
      string filter = "Configuration files (.clang-format)|*.clang-format";

      string path = SaveFile(fileName, defaultExt, filter);
      if (string.IsNullOrEmpty(path) == false)
      {
        WriteContentToFile(path, CreateOutput().ToString());
      }
    }

    private StringBuilder CreateOutput()
    {
      var output = new StringBuilder();
      output.AppendLine("---");

      foreach (var item in FormatOptionsData.FormatOptions)
      {
        var styleOption = string.Empty;
        if (item is FormatOptionToggleModel)
        {
          var option = item as FormatOptionToggleModel;
          styleOption = string.Concat(option.Name, ": ", option.IsEnabled.ToString().ToLower());
        }
        else if (item is FormatOptionModel)
        {
          var option = item as FormatOptionModel;
          styleOption = string.Concat(option.Name, ": ", option.Input);
        }

        output.AppendLine(styleOption);
      }

      return output;
    }

    #endregion
  }
}
