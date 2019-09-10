using System;

namespace ClangPowerTools.Helpers
{
  public class RegistryUtility
  {
    #region MyRegion

    private readonly string registerName;

    #endregion

    #region Constructor

    public RegistryUtility(string name) => registerName = name;

    #endregion

    #region Public Methods

    public string ReadRegistryKey(string value)
    {
      try
      {
        var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(registerName);
        var keyValue = key.GetValue(value).ToString();
        key.Close();
        return keyValue;
      }
      catch (Exception)
      {
        return null;
      }
    }

    public bool WriteRegistryKey(string keyName, string keyValue)
    {
      try
      {
        var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(registerName);
        key.SetValue(keyName, keyValue);
        key.Close();

        return true;
      }
      catch (Exception)
      {
        return false;
      }
    }

    #endregion

  }
}
