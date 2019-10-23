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
    /// <summary>
    /// Check if the user license is valid
    /// </summary>
    /// <returns>True if the license is valid. False otherwise</returns>
    Task<bool> ValidateAsync();

    /// <summary>
    /// Get the user license token value
    /// </summary>
    /// <returns>User license token value</returns>
    string GetToken();
  }
}
