using ClangPowerToolsShared.MVVM.Constants;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClangPowerToolsShared.MVVM.Models
{
  public class IconModel
  {
    public string IconPath { get; set; }
    public string Visibility { get; set; }
    public bool IsEnabled { get; set; }

    public IconModel(string iconPath, string visibility = "Hidden", bool isEnabled = false)
    {
      IconPath = iconPath;
      Visibility = visibility;
    }
  }
}
