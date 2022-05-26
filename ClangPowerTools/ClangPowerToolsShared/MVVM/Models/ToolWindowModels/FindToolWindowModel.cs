using System.ComponentModel;

namespace ClangPowerToolsShared.MVVM.Models.ToolWindowModels
{
  public class FindToolWindowModel : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;
    private bool isRunning = false;
    public DefaultArgsModel DefaultArgs { get; set; } = new DefaultArgsModel();

    public bool IsRunning
    {
      get
      {
        return isRunning;
      }
      set
      {
        isRunning = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsRunning"));
      }
    }
  }
}
