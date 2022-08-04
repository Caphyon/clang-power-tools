using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ClangPowerToolsShared.Helpers
{
  public static class CryptographyAlgo
  {
    public static string HashFile(string path)
    {
      using (var sha256 = SHA256.Create())
      {
        using (var stream = File.OpenRead(path))
        {
          return Encoding.UTF8.GetString(sha256.ComputeHash(stream));
        }
      }
    }
  }
}
