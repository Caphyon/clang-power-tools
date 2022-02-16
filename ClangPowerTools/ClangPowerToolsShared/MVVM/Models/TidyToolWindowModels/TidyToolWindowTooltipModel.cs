using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ClangPowerToolsShared.MVVM.Models.TidyToolWindowModels
{
  public class TidyToolWindowTooltipModel
  {
    public event PropertyChangedEventHandler PropertyChanged;

    private string removeTooltip = string.Empty;
    private string discardTooltip = string.Empty;
    private string tidyTooltip = string.Empty;
    private string fixTooltip = string.Empty;

    public string RemoveTooltip
    {
      get { return removeTooltip; }
      set
      {
        removeTooltip = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RemoveTooltip"));
      }
    }

    public string DiscardTooltip
    {
      get { return discardTooltip; }
      set
      {
        discardTooltip = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DiscardTooltip"));
      }
    }

    public string FixTooltip
    {
      get { return fixTooltip; }
      set
      {
        fixTooltip = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FixTooltip"));
      }
    }

    public string TidyTooltip
    {
      get { return tidyTooltip; }
      set
      {
        tidyTooltip = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TidyTooltip"));
      }
    }

    public void UpdateTooltips(CountFilesModel countFilesModel)
    {
      TidyTooltip = "test tidy" + countFilesModel.TotalCheckedFiles;
      RemoveTooltip = "test remove" + countFilesModel.TotalCheckedFiles;
      FixTooltip = "test fix" + countFilesModel.TotalCheckedFiles;
      DiscardTooltip = "test discard" + countFilesModel.TotalCheckedFiles;
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TotalChecked"));
    }

  }
}
