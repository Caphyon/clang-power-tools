using Microsoft.VisualStudio.Shell;
using System.IO;

namespace ClangPowerTools
{
  public class ConfigurationPage<TSettings> : DialogPage  where TSettings : new()
  {
    protected TSettings LoadFromFile(string aFilePath)
    {
      XmlSerializer serializer = new XmlSerializer();

      var config = File.Exists(aFilePath)
        ? serializer.DeserializeFromFIle<TSettings>(aFilePath)
        : new TSettings();

      return config;
    }

    protected void SaveToFile(string aFilePath, TSettings config)
    {
      XmlSerializer serializer = new XmlSerializer();
      serializer.SerializeToFile(aFilePath, config);
    }

  }
}
