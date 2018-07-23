//using ClangPowerTools.SilentFile;
//using Microsoft.VisualStudio.Shell;
//using System;
//using System.Collections.Generic;

//namespace ClangPowerTools
//{
//  public class SilentFileChangerGuard : IDisposable
//  {
//    #region Members

//    private HashSet<SilentFileChanger> mFileChangers = 
//      new HashSet<SilentFileChanger>(new SilentFileChangerEqualityComparer());

//    #endregion


//    #region Constructor

//    public SilentFileChangerGuard() { }


//    #endregion


//    #region Public methods



//    public void Add(SilentFileChanger aFileChanger)
//    {
//      aFileChanger.Suspend();
//      mFileChangers.Add(aFileChanger);
//    }

//    public void AddRange(AsyncPackage aServiceProvider, IEnumerable<string> aFilesPath)
//    {
//      foreach (string filePath in aFilesPath)
//        this.Add(new SilentFileChanger(aServiceProvider, filePath, true));
//    }


//    #endregion


//    #region IDisposable Implementation


//    public void Dispose()
//    {
//      foreach (SilentFileChanger file in mFileChangers)
//        file.Resume();
//    }


//    #endregion

//  }
//}
