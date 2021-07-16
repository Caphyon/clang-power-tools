using EnvDTE;

namespace ClangPowerTools.Helpers
{
  /// <summary>
  /// Contains the logic of managing Visual Studio window objects type
  /// </summary>
  public static class VsWindowController
  {
    #region Members

    /// <summary>
    /// Get the previouse focused window from Visual Studio
    /// </summary>
    public static Window PreviousWindow { get; private set; }

    #endregion


    #region Methods

    /// <summary>
    /// Set the previouse focused window from Visual Studio
    /// </summary>
    /// <param name="previousWindow"></param>
    public static void SetPreviousActiveWindow(Window previousWindow) => PreviousWindow = previousWindow;

    /// <summary>
    /// Moves the focus to the current item
    /// </summary>
    /// <param name="window">The window which will be focused</param>
    public static void Activate(Window window)
    {
      if (PreviousWindow == null)
        return;

      window.Activate();
    }
    #endregion

  }
}
