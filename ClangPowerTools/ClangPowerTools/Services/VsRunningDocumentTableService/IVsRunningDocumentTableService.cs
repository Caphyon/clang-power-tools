using Microsoft.VisualStudio.Shell.Interop;
using System.Threading;

namespace ClangPowerTools.Services
{
  public interface IVsRunningDocumentTableService
  {
    System.Threading.Tasks.Task<IVsRunningDocumentTable> GetVsRunningDocumentTableAsync(Microsoft.VisualStudio.Shell.IAsyncServiceProvider provider, CancellationToken cancellationToken);
  }
}
