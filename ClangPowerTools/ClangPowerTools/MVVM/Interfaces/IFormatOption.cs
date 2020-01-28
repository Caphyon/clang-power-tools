namespace ClangPowerTools.MVVM.Interfaces
{
  public interface IFormatOption
  {
    public string Name { get; set; }
    public string Description { get; set; }
    public bool HasToogleButton { get; }
    public bool HasTextBox { get; }
  }
}
