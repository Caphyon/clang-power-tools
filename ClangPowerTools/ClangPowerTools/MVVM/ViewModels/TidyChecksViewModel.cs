using ClangPowerTools.Views;
using System.Collections.Generic;
using System.ComponentModel;

namespace ClangPowerTools
{
  class TidyChecksViewModel: INotifyPropertyChanged
  {
    // public static List<TidyCheckModel> TidyChecksList { get; set; } = TidyChecks.predefinedChecks;

    #region Members
    public event PropertyChangedEventHandler PropertyChanged;

    #endregion

    public List<TidyCheckModel> TidyChecksList
    {
      get
      {
        return TidyChecks.checks; ;
      }
      set
      {
        TidyChecks.checks = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TidyChecksList"));
      }
    }
  }
}
