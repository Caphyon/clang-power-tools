using ClangPowerTools.MVVM.Models;
using System.IO;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace ClangPowerTools
{
  public class YAMLHandler
  {
    public async static Task ReadYAMLAsync(string path)
    {
      using var reader = new StreamReader(path);
      var input = await reader.ReadToEndAsync();

      var yaml = new YamlStream();
      yaml.Load(new StringReader(input));
      var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;

      var formatOptions = FormatOptionsAllData.FormatOptions;
      foreach (var entry in mapping.Children)
      {
        var option = new FormatOptionModel();
        if (formatOptions.TryGetValue(entry.Key.ToString(), out option))
        {
          var test = option;
        }

        //output.WriteLine(((YamlScalarNode)entry.Key).Value);
      }

      // List all the items
      //var items = (YamlSequenceNode)mapping.Children[new YamlScalarNode("items")];
      //foreach (YamlMappingNode item in items)
      //{
      //  output.WriteLine(
      //      "{0}\t{1}",
      //      item.Children[new YamlScalarNode("part_no")],
      //      item.Children[new YamlScalarNode("descrip")]
      //  );
      //}

    }



  }
}
