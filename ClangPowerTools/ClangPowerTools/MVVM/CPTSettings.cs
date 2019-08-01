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

    private SettingsPathBuilder settingsPathBuilder = new SettingsPathBuilder();

    private readonly string SettingsFileName = "cpt_settings.json";
    private readonly string GeneralConfigurationFileName = "GeneralConfiguration.config";
    private readonly string FormatConfigurationFileName = "FormatConfiguration.config";
    private readonly string TidyOptionsConfigurationFileName = "TidyOptionsConfiguration.config";
    private readonly string TidyPredefinedChecksConfigurationFileName = "TidyPredefinedChecksConfiguration.config";

    public void SerializeSettings()
    {
      List<object> models = new List<object>();
      models.Add(CompilerSettings);
      models.Add(FormatSettings);

      string path = settingsPathBuilder.GetPath(SettingsFileName);
      using (StreamWriter file = File.CreateText(path))
      {
        JsonSerializer serializer = new JsonSerializer();
        serializer.Formatting = Formatting.Indented;
        serializer.Serialize(file, models);
      }
    }

    public void DeserializeSettings()
    {
      string path = settingsPathBuilder.GetPath(SettingsFileName);
      using (StreamReader sw = new StreamReader(path))
      {
        string json = sw.ReadToEnd();
        JsonSerializer serializer = new JsonSerializer();
        List<object> models = JsonConvert.DeserializeObject<List<Object>>(json);

        CompilerSettingsModel test = JsonConvert.DeserializeObject<CompilerSettingsModel>(models[0].ToString());
      }
    }

    public void CheckOldSettings()
    {
      string path = settingsPathBuilder.GetPath(GeneralConfigurationFileName);

      if (File.Exists(path))
      {
        ClangOptions clangOptions = new ClangOptions();
        LoadFromFile(path, ref clangOptions);
        MapClangOptionsToCompilerSettings(clangOptions);
      }

      SerializeSettings();
    }


    public void DeleteOldSettings()
    {
      string path = settingsPathBuilder.GetPath("");
      string[] files = Directory.GetFiles(path, "*.config");
      foreach (var file in files)
      {
        File.Delete(file);
      }
    }

    public void LoadFromFile<TSettings>(string path, ref TSettings config) where TSettings : new()
    {
      XmlSerializer serializer = new XmlSerializer();
      config = serializer.DeserializeFromFile<TSettings>(path);
    }

    private void MapClangOptionsToCompilerSettings(ClangOptions clangOptions)
    {
      CompilerSettings.CompileFlags = clangOptions.ClangFlagsCollection;
      CompilerSettings.FilesToIgnore = clangOptions.FilesToIgnore;
      CompilerSettings.ProjectsToIgnore = clangOptions.ProjectsToIgnore;
      CompilerSettings.AdditionalIncludes = clangOptions.AdditionalIncludes;
      CompilerSettings.WarningsAsErrors = clangOptions.TreatWarningsAsErrors;
      CompilerSettings.ContinueOnError = clangOptions.Continue;
      CompilerSettings.ClangCompileAfterMSCVCompile = clangOptions.ClangCompileAfterVsCompile;
      CompilerSettings.VerboseMode = clangOptions.VerboseMode;
      CompilerSettings.Version = clangOptions.Version;
    }
  }
}
