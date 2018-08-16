namespace ClangPowerTools.Error
{
  public interface IBuilder<T>
  {
    void Build();
    T GetResult();
  }
}
