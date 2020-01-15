using System;

namespace ClangPowerTools.Helpers
{
  public class RegistryUtility
  {
    #region MyRegion

    private readonly string registryName;

    #endregion

    #region Constructor

    public RegistryUtility(string name) => registryName = name;

    #endregion

    #region Public Methods

    public string ReadKey(string keyName)
    {
      try
      {
        var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(registryName);
        var keyValue = key.GetValue(keyName).ToString();
        key.Close();
        return keyValue;
      }
      catch (Exception)
      {
        return null;
      }
    }

    public bool WriteKey(string keyName, string keyValue)
    {
      try
      {
        var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(registryName);
        key.SetValue(keyName, keyValue);
        key.Close();

        return true;
      }
      catch (Exception)
      {
        return false;
      }
    }

    public bool Exists() => Microsoft.Win32.Registry.CurrentUser.OpenSubKey(registryName) != null;

    #endregion

  }
}
