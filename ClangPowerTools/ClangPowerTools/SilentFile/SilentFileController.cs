using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System.Collections.Generic;
using System.IO;

namespace ClangPowerTools.SilentFile
{
  // Prevent visual studio to ask you if you want to reload the files
  class SilentFileController
  {
    #region New guard instance

    public SilentFileChangerGuard GetSilentFileChangerGuard() => new SilentFileChangerGuard();

    #endregion


    #region Public methods


    public void SilentFiles(AsyncPackage aServiceProvider, SilentFileChangerGuard aGuard, IEnumerable<string> aFilesPath)
    {
      // silent all selected files
      aGuard.AddRange(aServiceProvider, aFilesPath);
    }

    public void SilentOpenFiles(AsyncPackage aServiceProvider, SilentFileChangerGuard aGuard, DTE2 aDte)
    {
      // silent all open documents
      foreach (Document doc in aDte.Documents)
        aGuard.Add(new SilentFileChanger(aServiceProvider, Path.Combine(doc.Path, doc.Name), true));
    }


    #endregion


  }
}
