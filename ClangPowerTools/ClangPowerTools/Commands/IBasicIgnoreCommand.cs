using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClangPowerTools.Commands
{
  interface IBasicIgnoreCommand<T>
  {
    void AddIgnoreFilesToSettings(List<string> documentsToIgnore, T settings);
  }
}
