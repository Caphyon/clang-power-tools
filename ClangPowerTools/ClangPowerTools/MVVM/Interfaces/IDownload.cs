using System.ComponentModel;
using System.Net;

namespace ClangPowerTools.MVVM.Interfaces
{
  public interface IDownload
  {
    void Download(string uri, DownloadProgressChangedEventHandler method);

    void DownloadCompleted(object sender, AsyncCompletedEventArgs e);
  }
}
