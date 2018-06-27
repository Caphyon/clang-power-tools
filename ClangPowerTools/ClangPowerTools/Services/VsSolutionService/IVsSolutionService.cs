using Microsoft.VisualStudio.Shell.Interop;
using System.Threading;

namespace ClangPowerTools.Services
{
  public interface IVsSolutionService
  {
    System.Threading.Tasks.Task<IVsSolution> GetVsSolutionAsync(Microsoft.VisualStudio.Shell.IAsyncServiceProvider provider, CancellationToken cancellationToken);
  }
}
