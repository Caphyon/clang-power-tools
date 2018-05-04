using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClangPowerTools
{
  public class HeaderFiltersValue
  {
    private string mSelectedValue = string.Empty;

    private List<string> mValues = new List<string>()
    {
      "aaa",
      "bbb",
      "ccc"
    };

    public string SelectedValue { get; set; }

    public List<string> Values { get; set; } = new List<string>()
    {
      "aaa",
      "bbb",
      "ccc"
    };

  }
}
