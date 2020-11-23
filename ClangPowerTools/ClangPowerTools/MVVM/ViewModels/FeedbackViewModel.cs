using ClangPowerTools.MVVM.Commands;
using System.Diagnostics;
using System.Windows.Input;

namespace ClangPowerTools
{
  public class FeedbackViewModel
  {

    #region Members

    private ICommand githubCommand;
    private ICommand websiteCommand;

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

    public ICommand GithubCommand
    {
      get => githubCommand ??= new RelayCommand(() => OpenGitHubFeedback(), () => CanExecute);
    }

    public ICommand WebsiteCommand
    {
      get => websiteCommand ??= new RelayCommand(() => OpenWebsiteFeedback(), () => CanExecute);
    }

    #endregion

    #region Methods

    private void OpenGitHubFeedback()
    {
      Process.Start(new ProcessStartInfo("https://github.com/Caphyon/clang-power-tools/issues/new"));
    }

    private void OpenWebsiteFeedback()
    {
      Process.Start(new ProcessStartInfo("https://clangpowertools.com/contact.html"));
    }

    #endregion
  }
}
