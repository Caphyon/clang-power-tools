using ClangPowerTools.MVVM.Interfaces;
using System.ComponentModel;

namespace ClangPowerTools.MVVM.Models
{
  public class FormatOptionModel : IFormatOption, INotifyPropertyChanged
  {

    #region Members

    public event PropertyChangedEventHandler PropertyChanged;

    private bool isEnabled = true;
    private string nameFontWeight = FormatConstants.NormalFontWeight;

    #endregion

    #region Properties
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Paramater { get; set; } = string.Empty;

    public bool IsEnabled
    {
      get
      {
        return isEnabled;
      }
      set
      {
        isEnabled = value;
        OnPropertyChanged("IsEnabled");
      }
    }

    public string NameFontWeight
    {
      get
      {
        return nameFontWeight;
      }
      set
      {
        nameFontWeight = value;
        OnPropertyChanged("NameFontWeight");
      }
    }

    public bool IsModifed { get; set; } = false;
    public bool HasBooleanCombobox { get; set; } = false;
    public bool HasInputTextBox { get; set; } = false;
    public bool HasMultipleInputTextBox { get; set; } = false;
    public bool HasMultipleToggle { get; set; } = false;

    #endregion

    #region Methods

    public void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
  }
}
