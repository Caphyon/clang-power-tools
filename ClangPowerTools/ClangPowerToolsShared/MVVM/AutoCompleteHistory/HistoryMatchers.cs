using System.Collections.Generic;
using System.ComponentModel;

namespace ClangPowerToolsShared.MVVM.AutoCompleteHistory
{
  public class HistoryMatchers : INotifyPropertyChanged
  {
    private List<string> historyMatchers = new();

    public event PropertyChangedEventHandler PropertyChanged;

    //public static List<string> AutoCompleteMatchersAndHistory 
    //{ get { return ASTMatchers.AutoCompleteMatchers; } }
  }
}
