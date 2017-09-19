using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
