using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClangPowerTools.Events
{
  public class ActiveDocumentEventArgs
  {
    public bool IsActiveDocument { get; set; }

    public ActiveDocumentEventArgs(bool isActiveDocument)
    {
      IsActiveDocument = isActiveDocument;
    }
  }
}
