using ClangPowerToolsShared.MVVM.Constants;
using System.ComponentModel;

namespace ClangPowerToolsShared.MVVM.Models
{
  class MessageModel
  {

    #region Members

    public event PropertyChangedEventHandler PropertyChanged;

    private string textMessage = string.Empty;  
    private string visibitily = UIElementsConstants.Hidden;

    #endregion

    #region Properities

    public string Visibility 
    { 
      get { return visibitily; }
      set
      {
        visibitily = value;
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
