using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ClangPowerTools
{
  public class CPTSettings
  {
    public static CompilerSettingsModel CompilerSettings { get; set; }
    public static FormatSettingsModel FormatSettings { get; set; }

    private string path = string.Empty;
    private SettingsPathBuilder settingsPathBuilder = new SettingsPathBuilder();

    private readonly string SettingsFileName = "cpt_settings.json";
    private readonly string GeneralConfigurationFileName = "GeneralConfiguration.config";
    private readonly string FormatConfigurationFileName = "FormatConfiguration.config";
    private readonly string TidyOptionsConfigurationFileName = "TidyOptionsConfiguration.config";
    private readonly string TidyPredefinedChecksConfigurationFileName = "TidyPredefinedChecksConfiguration.config";

    public CPTSettings()
    {
      path = settingsPathBuilder.GetPath("cpt_settings.json");
    }


    public void SerializeSettings()
    {
      List<object> models = new List<object>();
      models.Add(CompilerSettings);
      models.Add(FormatSettings);

      using (StreamWriter file = File.CreateText(@"D:\path.json"))
      {
        JsonSerializer serializer = new JsonSerializer();
        serializer.Formatting = Formatting.Indented;
        //serialize object directly into file stream
        serializer.Serialize(file, models);
      }

      DeserializeSettings();
    }

    public void DeserializeSettings()
    {
      using(StreamReader sw = new StreamReader(@"D:\path.json"))
      {
        string json = sw.ReadToEnd();
        JsonSerializer serializer = new JsonSerializer();
        List<object> models = JsonConvert.DeserializeObject<List<Object>>(json);

        CompilerSettingsModel test = JsonConvert.DeserializeObject<CompilerSettingsModel>(models[0].ToString());
      }
    }

    public void CheckOldSettings()
    {
      path = settingsPathBuilder.GetPath(GeneralConfigurationFileName);

      if(File.Exists(path))
      {

      }
    }

    public TSettings LoadFromFile<TSettings>(string aFilePath, TSettings config) where TSettings : new()
    {
      XmlSerializer serializer = new XmlSerializer();
      config = serializer.DeserializeFromFile<TSettings>(aFilePath);
      return config;
    }

  }
}
