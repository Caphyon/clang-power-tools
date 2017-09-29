using System;

namespace ClangPowerTools
{
  [AttributeUsage(AttributeTargets.Property)]
  public class ClangCheckAttribute : Attribute
  {
    public bool IsFlag { get; private set; }
    public ClangCheckAttribute(bool aIsFlag)
    {
      IsFlag = aIsFlag;
    }
  }
}
