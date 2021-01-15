using ClangPowerTools.MVVM.Interfaces;
using ClangPowerTools.MVVM.Models;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace ClangPowerTools
{
  public class YAMLHandler
  {
    //TODO parsing should be handled by a different class
    public async static Task ReadYAMLAsync(string path)
    {
      using var reader = new StreamReader(path);
      var input = await reader.ReadToEndAsync();

      //TODO read directly in yaml LOAD
      var yaml = new YamlStream();
      yaml.Load(new StringReader(input));

      var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
      foreach (var entry in mapping.Children)
      {
        //TODO handle if not found, add empty node
        if (FormatOptionsAllData.FormatOptions.TryGetValue(entry.Key.ToString(), out IFormatOption option))
        {
          switch (option)
          {
            case FormatOptionToggleModel toggleModel:
              Enum.TryParse(entry.Value.ToString(), out ToggleValues value);
              toggleModel.BooleanCombobox = value;
              break;

            case FormatOptionInputModel inputModel:
              var inputValue = entry.Value.ToString();
              //TODO check on file creation
              if (inputValue.Contains('^') || inputValue.Length == 0)
              {
                inputModel.Input = string.Concat("'", inputValue, "'");
                break;
              }
              inputModel.Input = inputValue;
              break;

            case FormatOptionMultipleToggleModel multipleToggleModel:
              var sequenceToggleNode = (YamlMappingNode)mapping.Children[new YamlScalarNode(option.Name)];
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
              break;

            case FormatOptionMultipleInputModel multipleInputModel:
              var sequenceInputNode = (YamlSequenceNode)mapping.Children[new YamlScalarNode(option.Name)];
              var sb = new StringBuilder();
              sb.AppendLine(string.Concat(option.Name, ":"));
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
              break;
            default:
              break;
          }
        }
      }


    }
  }
}
