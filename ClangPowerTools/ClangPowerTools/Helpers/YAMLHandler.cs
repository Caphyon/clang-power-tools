using ClangPowerTools.MVVM.Interfaces;
using ClangPowerTools.MVVM.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using YamlDotNet.RepresentationModel;

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
      foreach (var entry in mapping.Children)
      {
        //TODO handle if not found, add empty node
        if (FormatOptionsAllData.FormatOptions.TryGetValue(entry.Key.ToString(), out IFormatOption option))
        {
          switch (option)
          {
            case FormatOptionToggleModel toggleModel:
              MapToggleModel(entry, toggleModel);
              break;

            case FormatOptionInputModel inputModel:
              MapInputModel(entry, inputModel);
              break;

            case FormatOptionMultipleToggleModel multipleToggleModel:
              MapMultipleToogleModel(entry, multipleToggleModel, option.Name);
              break;

            case FormatOptionMultipleInputModel multipleInputModel:
              MapMultipleInputModel(entry, multipleInputModel, option.Name);
              break;
            default:
              break;
          }
        }
      }
    }

    private void MapMultipleInputModel(KeyValuePair<YamlNode, YamlNode> entry, FormatOptionMultipleInputModel multipleInputModel, string name)
    {
      var sequenceInputNode = (YamlSequenceNode)mapping.Children[new YamlScalarNode(name)];
      var sb = new StringBuilder();
      sb.AppendLine(string.Concat(name, ":"));
      foreach (var node in sequenceInputNode)
      {
        switch (node.NodeType)
        {
          case YamlNodeType.Mapping:
            //TODO see if elements like '' are removed when exporting .clang-format
            var mappingNode = (YamlMappingNode)node;
            for (int i = 0; i < mappingNode.Children.Count; i++)
            {
              var nodeName = mappingNode.Children[i].Key.ToString();
              var nodeValue = mappingNode.Children[i].Value.ToString();
              if (nodeValue.Contains('^'))
              {
                nodeValue = string.Concat("'", nodeValue, "'");
              }

              if (i == 0)
              {
                sb.AppendLine(string.Concat("  - ", nodeName, ": ", nodeValue));
              }
              else
              {
                sb.AppendLine(string.Concat("    ", nodeName, ": ", nodeValue));
              }
            }
            break;
          case YamlNodeType.Scalar:
            sb.AppendLine(string.Concat("  - " + ((YamlScalarNode)node).Value));
            break;
          default:
            break;
        }
      }
      multipleInputModel.MultipleInput = sb.ToString().TrimEnd('\r', '\n');
    }

    private void MapMultipleToogleModel(KeyValuePair<YamlNode, YamlNode> entry, FormatOptionMultipleToggleModel multipleToggleModel, string name)
    {
      var sequenceToggleNode = (YamlMappingNode)mapping.Children[new YamlScalarNode(name)];
      foreach (var node in sequenceToggleNode)
      {
        foreach (var item in multipleToggleModel.ToggleFlags)
        {
          if (item.Name == node.Key.ToString())
          {
            Enum.TryParse(entry.Value.ToString(), out ToggleValues toogleValue);
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
      Enum.TryParse(entry.Value.ToString(), out ToggleValues value);
      toggleModel.BooleanCombobox = value;
    }

    #endregion
  }
}
