using System;
using System.Security;

namespace ClangPowerTools
{
  public class UserModel : IDisposable
  {
    #region Properties

    public string email { get; set; }
    public string password { get; set; }
    #endregion

    #region Constructor

    public UserModel(string email, string password)
    {
      this.email = email;
      this.password = password;
    }

    public UserModel() { }

    public void Dispose()
    {
      email = string.Empty;
      password = string.Empty;
    }

    #endregion
  }
}
