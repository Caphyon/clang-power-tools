namespace ClangPowerTools
{
  public class UserModel
  {
    public string email { get; set; }
    public string password { get; set; }
    public UserModel(string email, string password)
    {
      this.email = email;
      this.password = password;
    }
  }
}
