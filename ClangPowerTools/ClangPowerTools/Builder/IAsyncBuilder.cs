using System.Threading.Tasks;

namespace ClangPowerTools.Builder
{
  public interface IAsyncBuilder<T>
  {
    Task AsyncBuild();

    T GetAsyncResult();
  }
}
