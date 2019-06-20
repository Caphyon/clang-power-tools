using System.ComponentModel;

namespace ClangPowerTools
{
  public class LoginViewModel : INotifyPropertyChanged
  {
    #region Members

    private string email;
    private string password;
    public event PropertyChangedEventHandler PropertyChanged;

    #endregion

    #region Properties

    public string Email
    {
      get { return email; }
      set
      {
        email = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Email"));
      }
    }

    public string Password
    {
      get { return password; }
      set
      {
        password = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Password"));
      }
    }

    #endregion

  }
}
