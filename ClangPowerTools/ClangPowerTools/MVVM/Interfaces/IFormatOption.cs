using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClangPowerTools.MVVM.Interfaces
{
  public interface IFormatOption
  {
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsToogleButton { get; set; }
    public bool IsTextBox { get; set; }
  }
}
