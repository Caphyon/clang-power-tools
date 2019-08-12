using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ClangPowerTools.Commands
{
  class BasicIgnoreCommand<T> : IBasicIgnoreCommand<T>
  {
    public void AddIgnoreFilesToSettings(List<string> documentsToIgnore, T settings)
    {
      if (!documentsToIgnore.Any())
      {
        return;
      }

      string filesToIgnore = (string)typeof(T).GetProperty("FilesToIgnore").GetValue(settings);
      if (filesToIgnore.Length > 0)
      {
        filesToIgnore += ";";
      }

      filesToIgnore += string.Join(";", RemoveDuplicateFiles(documentsToIgnore, filesToIgnore));
      PropertyInfo propertyInfo = settings.GetType().GetProperty("FilesToIgnore");
      propertyInfo.SetValue(settings, Convert.ChangeType(filesToIgnore, propertyInfo.PropertyType), null);

    }

    private List<string> RemoveDuplicateFiles(List<string> documentsToIgnore, string filesToIgnore)
    {
      List<string> trimmedDocumentToIgnore = new List<string>();

      foreach (var item in documentsToIgnore)
      {
        if (!filesToIgnore.Contains(item))
        {
          trimmedDocumentToIgnore.Add(item);
        }
      }
      return trimmedDocumentToIgnore;
    }
  }
}
