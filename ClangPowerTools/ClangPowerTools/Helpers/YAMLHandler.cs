using ClangPowerTools.MVVM.Interfaces;
using ClangPowerTools.MVVM.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace ClangPowerTools
{
  public class YAMLHandler
  {
    #region Members

    private YamlMappingNode mapping;

    #endregion

    #region Public Methods 

    public void ReadFormatOptionsYaml(string path)
    {
      using var reader = new StreamReader(path);
      var input = reader.ReadToEnd();

      var yaml = new YamlStream();
      yaml.Load(new StringReader(input));

      mapping = (YamlMappingNode)yaml.Documents[0].RootNode;

      MapToFormatOptions();
    }

    #endregion

    #region Private Methods

    private void MapToFormatOptions()
    {
      FormatOptionsAllData.DisableAllOptions();
      foreach (var entry in mapping.Children)
      {
        if (FormatOptionsAllData.FormatOptions.TryGetValue(entry.Key.ToString(), out IFormatOption option))
        {
          switch (option)
          {
            case FormatOptionToggleModel toggleModel:
              MapToggleModel(entry, toggleModel);
              toggleModel.IsEnabled = true;
              break;

            case FormatOptionInputModel inputModel:
              MapInputModel(entry, inputModel);
              inputModel.IsEnabled = true;
              break;

            case FormatOptionMultipleToggleModel multipleToggleModel:
              MapMultipleToogleModel(multipleToggleModel, option.Name);
              multipleToggleModel.IsEnabled = true;
              break;

            case FormatOptionMultipleInputModel multipleInputModel:
              MapMultipleInputModel(multipleInputModel, option.Name);
              multipleInputModel.IsEnabled = true;
              break;
            default:
              break;
          }
        }
      }
    }

    private void MapMultipleInputModel(FormatOptionMultipleInputModel multipleInputModel, string name)
    {
      var sequenceInputNode = (YamlSequenceNode)mapping.Children[new YamlScalarNode(name)];
      var serializer = new SerializerBuilder().Build();
      var yaml = serializer.Serialize(sequenceInputNode);
      multipleInputModel.MultipleInput = yaml.TrimEnd(Environment.NewLine);
    }

    private void MapMultipleToogleModel(FormatOptionMultipleToggleModel multipleToggleModel, string name)
    {
      var sequenceToggleNode = (YamlMappingNode)mapping.Children[new YamlScalarNode(name)];
      foreach (var node in sequenceToggleNode)
      {
        foreach (var item in multipleToggleModel.ToggleFlags)
        {
          if (item.Name == node.Key.ToString())
          {
            Enum.TryParse(node.Value.ToString(), true, out ToggleValues toogleValue);
            item.Value = toogleValue;
            break;
          }
        }
      }
    }

    private void MapInputModel(KeyValuePair<YamlNode, YamlNode> entry, FormatOptionInputModel inputModel)
    {
      var inputValue = entry.Value.ToString();
      if (inputValue.Contains('^') || inputValue.Length == 0)
      {
        inputModel.Input = string.Concat("'", inputValue, "'");
        return;
      }
      inputModel.Input = inputValue;
    }

    private void MapToggleModel(KeyValuePair<YamlNode, YamlNode> entry, FormatOptionToggleModel toggleModel)
    {
      Enum.TryParse(entry.Value.ToString(), true, out ToggleValues value);
      toggleModel.BooleanCombobox = value;
    }

    #endregion
  }
}
