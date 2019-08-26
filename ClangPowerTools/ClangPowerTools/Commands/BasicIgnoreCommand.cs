using ClangPowerTools.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace ClangPowerTools.Commands
{
  public class BasicIgnoreCommand<T> : IBasicIgnoreCommand<T>//, BasicCommand
  {
    public void AddIgnoreFilesToSettings(List<string> documentsToIgnore, T settings)
    {
      if (!documentsToIgnore.Any())
      {
        return;
      }

      string filesToIgnore = (string) ReflectionManager.GetProperty<T>(settings, "FilesToIgnore");

      if (filesToIgnore.Length > 0)
      {
        filesToIgnore += ";";
      }

      filesToIgnore += string.Join(";", RemoveDuplicateFiles(documentsToIgnore, filesToIgnore));
      ReflectionManager.SetProperty(settings, "FilesToIgnore", filesToIgnore);
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
