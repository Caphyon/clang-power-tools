using ClangPowerToolsShared.MVVM.Constants;
using System.ComponentModel;

namespace ClangPowerToolsShared.MVVM.Models
{
  public class MessageModel
  {

    #region Members

    public event PropertyChangedEventHandler PropertyChanged;

    private string textMessage = string.Empty;  
    private string visibility = UIElementsConstants.Hidden;

    #endregion

    #region Properities

    public string Visibility 
    { 
      get { return visibility; }
      set
      {
        visibility = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Visibility"));
      }
    }

    public string TextMessage 
    { get { return textMessage; }
      set
      {
        textMessage = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TextMessage"));
      }
    }

    #endregion

  }
}
