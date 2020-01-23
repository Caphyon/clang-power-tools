using ClangPowerTools.MVVM.Interfaces;
using System.ComponentModel;

namespace ClangPowerTools
{
  public class FormatOptionsViewModel : CommonSettingsFunctionality, INotifyPropertyChanged
  {
    #region Members

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion


    #region Properties

    public IFormatOption MyProperty { get; set; }

    #endregion
  }
}
