﻿using ClangPowerTools.Options.ViewModel;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace ClangPowerTools.DialogPages
{
  public class ClangFormatOptionsView : ConfigurationPage<ClangFormatOptions>
  {
    protected override IWin32Window Window
    {
      get
      {
        ElementHost elementHost = new ElementHost();
        elementHost.Child = new ClangFormatOptionsUserControl(this);
        return elementHost;


        //ClangFormatOptionsUserControl page = new ClangFormatOptionsUserControl(this);
        //return page;
      }
    }


    #region Members

    private const string kGeneralSettingsFileName = "FormatConfiguration.config";
    private SettingsPathBuilder mSettingsPathBuilder = new SettingsPathBuilder();

    #endregion

    #region Properties 
    

    #region Format On Save

    [Category("Format On Save")]
    [DisplayName("Enable")]
    [Description("Enable running clang-format when modified files are saved. " +
      "Will only format if Style is found (ignores Fallback Style).")]
    public bool EnableFormatOnSave { get; set; }

    [Category("Format On Save")]
    [DisplayName("File extensions")]
    [Description("When formatting on save, clang-format will be applied only to files with these extensions.")]
    public string FileExtensions { get; set; }

    [Category("Format On Save")]
    [DisplayName("Skip files")]
    [Description("When formatting on save, clang-format will not be applied on these files.")]
    public string SkipFiles { get; set; }

    #endregion

    #region Format Options

    [Category("Format Options")]
    [DisplayName("Assume filename")]
    [Description("When reading from stdin, clang-format assumes this filename to look for a style config file" +
      "(with -style=file) and to determine the language.")]
    public string AssumeFilename { get; set; }

    [Category("Format Options")]
    [DisplayName("Fallback style")]
    [Description("The name of the predefined style used as a fallback in case clang-format is invoked with " +
      "-style=file, but can not find the .clang-format file to use.\nUse -fallback-style=none to skip formatting.")]
    public ClangFormatFallbackStyle? FallbackStyle { get; set; }

    //[Category("Format Options")]
    //[DisplayName("Sort includes")]
    //[Description("If set, overrides the include sorting behavior determined by the SortIncludes style flag.")]
    //public bool SortIncludes { get; set; }

    [Category("Format Options")]
    [DisplayName("Style")]
    [Description("Coding style, currently supports: LLVM, Google, Chromium, Mozilla, WebKit.\nUse -style=file to load " +
      "style configuration from .clang-format file located in one of the parent directories of the " +
      "source file(or current directory for stdin).\nUse -style=\"{key: value, ...}\" to set specific parameters, " +
      "e.g.: -style=\"{BasedOnStyle: llvm, IndentWidth: 8}\"")]
    public ClangFormatStyle? Style { get; set; }

    #endregion

    public override void SaveSettingsToStorage()
    {
      string path = mSettingsPathBuilder.GetPath(kGeneralSettingsFileName);

      var updatedConfig = LoadFromFile(path);
      updatedConfig.EnableFormatOnSave = this.EnableFormatOnSave;
      updatedConfig.FileExtensions = this.FileExtensions;
      updatedConfig.SkipFiles = this.SkipFiles;
      updatedConfig.AssumeFilename = this.AssumeFilename;
      updatedConfig.FallbackStyle = this.FallbackStyle;

      //updatedConfig.SortIncludes = this.SortIncludes;

      updatedConfig.Style = this.Style;

      SaveToFile(path, updatedConfig);
    }

    public override void LoadSettingsFromStorage()
    {
      string path = mSettingsPathBuilder.GetPath(kGeneralSettingsFileName);
      var loadedConfig = LoadFromFile(path);

      this.EnableFormatOnSave = loadedConfig.EnableFormatOnSave;

      this.FileExtensions = null == loadedConfig.FileExtensions ?
        DefaultOptions.kFileExtensions : loadedConfig.FileExtensions;

      this.SkipFiles = null == loadedConfig.SkipFiles ?
        DefaultOptions.kSkipFiles : loadedConfig.SkipFiles;

      this.AssumeFilename = loadedConfig.AssumeFilename;

      this.FallbackStyle = null == loadedConfig.FallbackStyle ? 
        ClangFormatFallbackStyle.none : loadedConfig.FallbackStyle;

      //this.SortIncludes = loadedConfig.SortIncludes;

      this.Style = null == loadedConfig.Style ? ClangFormatStyle.file : loadedConfig.Style;
    }

    #endregion

    public ClangFormatOptionsView Clone()
    {
      // Use MemberwiseClone to copy value types.
      var clone = (ClangFormatOptionsView)MemberwiseClone();
      return clone;
    }

  }
}
