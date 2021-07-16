using System;

namespace ClangPowerTools
{
  [AttributeUsage(AttributeTargets.Property)]
  public class ClangTidyPathAttribute : Attribute
  {
    public bool Activate { get; set; }

    public ClangTidyPathAttribute(bool aActivate)
    {
      Activate = aActivate;
    }
  }
}
