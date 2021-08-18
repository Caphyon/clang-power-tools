using System;

namespace ClangPowerTools
{
  [AttributeUsage(AttributeTargets.Property)]
  public class ClangFormatPathAttribute : Attribute
  {
    public bool Activate { get; set; }

    public ClangFormatPathAttribute(bool aActivate)
    {
      Activate = aActivate;
    }
  }
}
