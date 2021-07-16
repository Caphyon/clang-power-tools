﻿using System;

namespace ClangPowerTools.Helpers
{
  public class RegistryUtility
  {
    #region MyRegion

    private readonly string registryName;

    #endregion

    #region Constructor

    public RegistryUtility(string registryName) => this.registryName = registryName;

    #endregion

    #region Public Methods


    public string ReadLocalMachineKey(string keyName)
    {
      try
      {
        using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(registryName);
        if (key == null) return null;
        var keyValue = key.GetValue(keyName).ToString();
        return keyValue;
      }
      catch (Exception)
      {
        return null;
      }
    }

    public string ReadCurrentUserKey(string keyName)
    {
      try
      {
        using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(registryName);
        var keyValue = key.GetValue(keyName)?.ToString();
        return keyValue;
      }
      catch (Exception)
      {
        return null;
      }
    }

    public bool WriteCurrentUserKey(string keyName, string keyValue)
    {
      try
      {
        using var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(registryName);
        key.SetValue(keyName, keyValue);
        return true;
      }
      catch (Exception)
      {
        return false;
      }
    }

    public bool Exists() => Microsoft.Win32.Registry.CurrentUser.OpenSubKey(registryName) != null;

    public bool DeleteCurrentUserKey(string keyName)
    {
      try
      {
        using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(registryName, true);
        if (key == null)
          return false;

        key.DeleteValue(keyName);
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
