using System;

namespace ClangPowerTools.MVVM.Interfaces
{
  public interface IInstall
  {
    void Install(string path);

    void Uninstall(string path);

    EventHandler InstallFinished { get; set; }
    EventHandler UninstallFinished { get; set; }
  }
}
