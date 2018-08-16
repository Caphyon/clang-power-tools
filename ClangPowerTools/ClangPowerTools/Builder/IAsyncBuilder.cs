using System.Threading.Tasks;

namespace ClangPowerTools.Builder
{
  public interface IAsyncBuilder<T>
  {
    Task<object> AsyncBuild();

    T GetAsyncResult();
  }
}
