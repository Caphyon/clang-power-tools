using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System.Threading;

namespace ClangPowerTools.Services
{
  public interface IEnvDTEService
  {
    System.Threading.Tasks.Task<DTE2> GetDTE2Async(IAsyncServiceProvider provider, CancellationToken cancellationToken);
  }
}
