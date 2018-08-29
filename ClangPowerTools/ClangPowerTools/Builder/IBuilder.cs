namespace ClangPowerTools.Builder
{
  public interface IBuilder<T>
  {
    void Build();
    T GetResult();
  }
}
