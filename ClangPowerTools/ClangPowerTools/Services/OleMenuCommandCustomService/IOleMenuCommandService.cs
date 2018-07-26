using Microsoft.VisualStudio.Shell;
using System.Threading;

namespace ClangPowerTools.Services.OleMenuCommandCustomService
{
  interface IOleMenuCommandService
  {
    System.Threading.Tasks.Task<OleMenuCommandService> GetVsFileChangeAsync(IAsyncServiceProvider provider, CancellationToken cancellationToken);
  }
}
