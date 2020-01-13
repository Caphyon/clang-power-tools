using ClangPowerTools.MVVM.Commands;
using System.Diagnostics;
using System.Windows.Input;

namespace ClangPowerTools
{
  public class ReleaseNotesViewModel
  {
    #region Members

    private ICommand upgradeCommand;
    private ICommand openBlogCommand;

    #endregion

    #region Properties

    public bool CanExecute
    {
      get
      {
        return true;
      }
    }

    #endregion

    #region Commands

    public ICommand Upgrade
    {
      get => upgradeCommand ?? (upgradeCommand = new RelayCommand(() => UpgradeAction(), () => CanExecute));
    }

    public ICommand OpenBlog
    {
      get => openBlogCommand ?? (openBlogCommand = new RelayCommand(() => OpenBlogAction(), () => CanExecute));
    }

    #endregion

    #region Private Methods

    private void UpgradeAction()
    {
      Process.Start(new ProcessStartInfo("https://clangpowertools.com/download.html"));
    }

    private void OpenBlogAction()
    {
      Process.Start(new ProcessStartInfo("https://clangpowertools.com/blog/future-of-clang-power-tools.html"));
    }

    #endregion
  }
}
