using ClangPowerTools.MVVM.LicenseValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClangPowerTools.MVVM.Interfaces
{
  public interface ILicense
  {
    bool Validate();

    string GetToken();
  }
}
