﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClangPowerTools.Events
{
  public class HasEncodingErrorEventArgs
  {
    public bool HasEncodingError { get; set; }

    public HasEncodingErrorEventArgs(bool hasEncodingError)
    {
      HasEncodingError = hasEncodingError;
    }
  }
}
