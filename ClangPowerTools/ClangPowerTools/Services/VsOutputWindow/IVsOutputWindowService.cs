using Microsoft.VisualStudio.Shell.Interop;
using System.Threading;

namespace ClangPowerTools.Services
{
  public interface IVsOutputWindowService
  {
    System.Threading.Tasks.Task<IVsOutputWindow> GetOutputWindowAsync(Microsoft.VisualStudio.Shell.IAsyncServiceProvider provider, CancellationToken cancellationToken);
  }
}
