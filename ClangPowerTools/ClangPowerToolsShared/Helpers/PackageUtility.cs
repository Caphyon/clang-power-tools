﻿using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace ClangPowerTools.Helpers
{
  public class PackageUtility
  {
    public static string GetVersion()
    {
      var assemblyPath = Assembly.GetExecutingAssembly().Location;
      assemblyPath = assemblyPath.Substring(0, assemblyPath.LastIndexOf('\\'));
      var manifestPath = Path.Combine(assemblyPath, "extension.vsixmanifest");

      if (!File.Exists(manifestPath))
        return string.Empty;

      var doc = new XmlDocument();
      doc.Load(manifestPath);
      var metaData = doc.DocumentElement.ChildNodes.Cast<XmlElement>().First(x => x.Name == "Metadata");
      var identity = metaData.ChildNodes.Cast<XmlElement>().First(x => x.Name == "Identity");

      return identity.GetAttribute("Version");
    }
  }
}
