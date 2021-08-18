using System.Threading.Tasks;

namespace ClangPowerTools.Builder
{
  public interface IBuilderAsync<T>
  {
    Task BuildAsync();

    T GetAsyncResult();
  }
}
