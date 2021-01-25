using ClangPowerTools.MVVM.Commands;
using ClangPowerTools.MVVM.Views;
using System.Diagnostics;
using System.Windows.Input;

namespace ClangPowerTools
{
  public class ReleaseNotesViewModel
  {
    #region Members

    private ICommand upgradeCommand;
    private ICommand openBlogCommand;
    private readonly ReleaseNotesView releaseNotesView;

    #endregion

    #region Constructor

    public ReleaseNotesViewModel(ReleaseNotesView release)
    {
      releaseNotesView = release;
    }

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

    public ICommand LogIn
    {
      get => upgradeCommand ?? (upgradeCommand = new RelayCommand(() => LogInAction(), () => CanExecute));
    }

    public ICommand OpenBlog
    {
      get => openBlogCommand ?? (openBlogCommand = new RelayCommand(() => OpenBlogAction(), () => CanExecute));
    }

    #endregion

    #region Private Methods

    private void LogInAction()
    {
      releaseNotesView.Close();
      LoginView loginView = new LoginView();
      loginView.ShowDialog();
    }

    private void OpenBlogAction()
    {
      Process.Start(new ProcessStartInfo("https://clangpowertools.com/blog/future-of-clang-power-tools.html"));
    }

    #endregion
  }
}
