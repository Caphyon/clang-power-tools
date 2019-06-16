namespace ClangPowerTools
{
  public class UserModel
  {
    #region Properties

    public string email { get; set; }
    public string password { get; set; }
    public bool IsActive { get; set; } = false;

    #endregion

    #region Constructor

    public UserModel(string email, string password)
    {
      this.email = email;
      this.password = password;
    }

    public UserModel()
    {
    }

    #endregion

  }
}
