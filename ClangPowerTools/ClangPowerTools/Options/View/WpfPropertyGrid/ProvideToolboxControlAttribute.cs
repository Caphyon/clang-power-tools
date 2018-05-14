using Microsoft.VisualStudio.Shell;
using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace ClangPowerTools.Options.View.WpfPropertyGrid
{
  /// <summary>
  /// This attribute adds a ToolboxControlsInstaller key for the assembly to install toolbox controls from the assembly.
  /// </summary>
  /// <remarks>
  /// For example
  ///     [$(Rootkey)\ToolboxControlsInstaller\$FullAssemblyName$]
  ///         "Codebase"="$path$"
  ///         "WpfControls"="1"
  /// </remarks>
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
  [ComVisible(false)]
  public sealed class ProvideToolboxControlAttribute : RegistrationAttribute
  {
    private const string ToolboxControlsInstallerPath = "ToolboxControlsInstaller";
    private readonly string name;
    private readonly bool areWPFControls;

    /// <summary>
    /// Creates a new ProvideToolboxControl attribute to register the assembly for toolbox controls installer.
    /// </summary>
    /// <param name="name">The name of the toolbox controls.</param>
    /// <param name="areWPFControls">Indicates whether the toolbox controls are WPF controls.</param>
    public ProvideToolboxControlAttribute(string name, bool areWPFControls)
    {
      if (name == null)
      {
        throw new ArgumentNullException(nameof(name));
      }

      this.name = name;
      this.areWPFControls = areWPFControls;
    }

    /// <summary>
    /// Called to register this attribute with the given context. The context
    /// contains the location where the registration information should be placed.
    /// It also contains other information such as the type being registered and path information.
    /// </summary>
    /// <param name="context">Given context to register in.</param>
    public override void Register(RegistrationContext context)
    {
      if (context == null)
      {
        throw new ArgumentNullException(nameof(context));
      }

      using (Key key = context.CreateKey(string.Format(CultureInfo.InvariantCulture, "{0}\\{1}",
                                                       ToolboxControlsInstallerPath,
                                                       context.ComponentType.Assembly.FullName)))
      {
        key.SetValue(string.Empty, this.name);
        key.SetValue("Codebase", context.CodeBase);
        if (this.areWPFControls)
        {
          key.SetValue("WPFControls", "1");
        }
      }
    }

    /// <summary>
    /// Called to unregister this attribute with the given context.
    /// </summary>
    /// <param name="context">A registration context provided by an external registration tool. 
    /// The context can be used to remove registry keys, log registration activity, and obtain information
    /// about the component being registered.</param>
    public override void Unregister(RegistrationContext context)
    {
      if (context != null)
      {
        context.RemoveKey(string.Format(CultureInfo.InvariantCulture, "{0}\\{1}",
                                                     ToolboxControlsInstallerPath,
                                                     context.ComponentType.Assembly.FullName));
      }
    }
  }
}
