using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ClangPowerTools.Convertors
{
  public enum ClangBinDetectionStatus
  {
    SystemPath,
    ProgramFiles,
    NotFound
  }

  public class ClangBinDirectory
  {
    public const string kAutodetectMarker = "Autodetected";
    private const string kClangBinary = "clang++.exe";

    private static readonly string[] kDefaultClangBinDirectories =
    {
      @"%ProgramW6432%\LLVM\bin",
      @"%ProgramFiles(x86)%\LLVM\bin"
    };

    private bool mIsAutodetected;
    private string mCustomPath;

    public ClangBinDirectory()
    {
      this.IsAutodetected = true;
    }

    public ClangBinDirectory(string path)
    {
      this.CustomPath = path;
    }

    public bool IsAutodetected
    {
      get => this.mIsAutodetected;
      set
      {
        this.mIsAutodetected = value;
        this.mCustomPath = String.Empty;
      }
    }

    public string CustomPath
    {
      get => this.mCustomPath;
      set
      {
        this.mIsAutodetected = false;
        this.mCustomPath = value;
      }
    }

    public string Serialize()
    {
      return this.IsAutodetected ? ClangBinDirectory.kAutodetectMarker : this.CustomPath;
    }

    public static ClangBinDirectory Deserialize(string value)
    {
      var path = new ClangBinDirectory();

      if (value != ClangBinDirectory.kAutodetectMarker)
        path.CustomPath = value;

      return path;
    }

    private static bool IsAccessibleFromSystemPath(string executable)
    {
      return Environment.
        GetEnvironmentVariable("PATH").
        Split(';').
        FirstOrDefault(
          pathDir => IsExecutableInDirectory(executable, pathDir)
        ) != null;
    }

    private static bool IsExecutableInDirectory(string executable, string dir) => File.Exists(Path.Combine(dir, executable));

    public static Tuple<ClangBinDetectionStatus, string> DetectClangBinDir()
    {
      if (IsAccessibleFromSystemPath(kClangBinary))
      {
        return Tuple.Create(ClangBinDetectionStatus.SystemPath, String.Empty);
      }
      else
      {
        var autodetectedInstallationDir = kDefaultClangBinDirectories.
          Select(dir => Environment.ExpandEnvironmentVariables(dir)).
          FirstOrDefault(dir => IsExecutableInDirectory(kClangBinary, dir));

        if (autodetectedInstallationDir != default(string))
        {
          return Tuple.Create(ClangBinDetectionStatus.ProgramFiles, autodetectedInstallationDir);
        }
        else
        {
          return Tuple.Create(ClangBinDetectionStatus.NotFound, String.Empty);
        }
      }
    }

    public String Override
    {
      get
      {
        if (this.IsAutodetected)
        {
          var autodetectionStatus = DetectClangBinDir();
          if (autodetectionStatus.Item1 == ClangBinDetectionStatus.ProgramFiles)
            return autodetectionStatus.Item2;
        }

        if (!String.IsNullOrEmpty(this.CustomPath))
          return this.CustomPath;

        return null;
      }
    }
  }

  public class ClangBinDirConvertor : ComboBoxConvertor
  {
    public ClangBinDirConvertor() 
    : base(new ArrayList {})
    { }

    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
      if (sourceType == typeof(string))
        return true;

      return base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
      string s = value as string;
      if (s == null)
        return base.ConvertFrom(context, culture, value);

      var convertedPath = new ClangBinDirectory();

      if (!s.StartsWith(ClangBinDirectory.kAutodetectMarker))
        convertedPath.CustomPath = s;

      return convertedPath;
    }

    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
      if (destinationType == typeof(string))
      {
        if (value == null)
          return String.Empty;

        if (value.GetType() == typeof(string))
          return value;

        if (value.GetType() == typeof(ClangBinDirectory))
        {
          var path = value as ClangBinDirectory;
          return path.IsAutodetected ? GetAutodetectionLabel() : path.CustomPath;
        }
      }

      return base.ConvertTo(context, culture, value, destinationType);
    }

    private String GetAutodetectionLabel()
    {
      var autodetectionLabel = ClangBinDirectory.kAutodetectMarker + " in ";

      var autodetectionStatus = ClangBinDirectory.DetectClangBinDir();

      switch (autodetectionStatus.Item1)
      {
        case ClangBinDetectionStatus.SystemPath:
          autodetectionLabel += "system %PATH%";
          break;

        case ClangBinDetectionStatus.ProgramFiles:
          autodetectionLabel += autodetectionStatus.Item2;
          break;

        case ClangBinDetectionStatus.NotFound:
          autodetectionLabel += "(not found)";
          break;

        default:
          throw new NotSupportedException();
      }

      return autodetectionLabel;
    }

    public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
    {
      return new StandardValuesCollection(
        new ArrayList
        {
          new ClangBinDirectory()
        } );
    }
  }
}
