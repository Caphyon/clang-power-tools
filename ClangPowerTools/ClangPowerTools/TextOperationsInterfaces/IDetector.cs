using System.Text.RegularExpressions;

namespace ClangPowerTools.TextOperationsInterfaces
{
  public interface IDetector
  {
    bool Detect(string aText, out Match aMatcheResult);

  }
}
