using System.ComponentModel;

namespace ClangPowerToolsShared.MVVM.Models.ToolWindowModels
{
  public class FindToolWindowModel : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;
    private bool isRunning = false;
    private string matcherDetails = string.Empty;
    public DefaultArgsModel DefaultArgsModel { get; set; } = new DefaultArgsModel();

    public FindToolWindowModel()
    {
       matcherDetails = DefaultArgsModel.Details;
    }

    public string MatcherDetails
    {
      get
      {
        return matcherDetails;
      }
      set
      {
        matcherDetails = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MatcherDetails"));
      }
    }

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
