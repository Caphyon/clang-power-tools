namespace ClangPowerToolsShared.MVVM.Interfaces
{
  public interface IViewMatcher
  {
    string Name { get; }
    int Id { get; }
    string Details { get; }
    string Visibility { get; }
    void Hide();
    void Show();
  }
}
