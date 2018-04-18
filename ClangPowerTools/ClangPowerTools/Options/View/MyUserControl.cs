using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClangPowerTools.DialogPages;

namespace ClangPowerTools.Options.ViewModel
{
  public partial class MyUserControl : UserControl
  {
    private ClangFormatOptionsView page;

    public MyUserControl(ClangFormatOptionsView page)
    {
      this.page = page;
      InitializeComponent();

      elementHost1.Child = new UserControlWPF(page);
    }
  }
}
