using EnvDTE;
using EnvDTE80;
using System;
using System.Collections.Generic;
using System.IO;

namespace ClangPowerTools.SilentFile
{
  // Prevent visual studio to ask you if you want to reload the files
  class SilentFileController
  {
    public SilentFileChangerGuard GetSilentFileChangerGuard() => new SilentFileChangerGuard();

    public void SilentFiles(IServiceProvider aServiceProvider, SilentFileChangerGuard aGuard, IEnumerable<string> aFilesPath)
    {
      // silent all selected files
      aGuard.AddRange(aServiceProvider, aFilesPath);
    }

    public void SilentOpenFiles(IServiceProvider aServiceProvider, SilentFileChangerGuard aGuard, DTE2 aDte)
    {
      // silent all open documents
      foreach (Document doc in aDte.Documents)
        aGuard.Add(new SilentFileChanger(aServiceProvider, Path.Combine(doc.Path, doc.Name), true));
    }


  }
}
