using System.Windows.Forms;
using ClangPowerTools.DialogPages;

namespace ClangPowerTools.Options.ViewModel
{
  public partial class ClangFormatOptionsUserControl : UserControl
  {
    private ClangFormatOptionsView page;

    public ClangFormatOptionsUserControl(ClangFormatOptionsView page)
    {
      this.page = page;
      InitializeComponent();

      clangFormatOptionsElementHost.Child = new ClangFormatOptionsUserControlWPF(page);
    }
  }
}
