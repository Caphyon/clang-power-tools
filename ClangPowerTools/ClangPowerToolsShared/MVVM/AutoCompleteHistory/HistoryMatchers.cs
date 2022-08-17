using System.Collections.Generic;
using System.ComponentModel;

namespace ClangPowerToolsShared.MVVM.AutoCompleteHistory
{
  public class HistoryMatchers : INotifyPropertyChanged
  {
    private Queue<string> historyMatchers = new();

    public event PropertyChangedEventHandler PropertyChanged;

    public void AddInHistory(string matcher)
    {
      historyMatchers.Enqueue(matcher);
    }
    //public static List<string> AutoCompleteMatchersAndHistory { get { return ASTMatchers.AutoCompleteMatchers; } }
  }
}
