namespace ClangPowerTools.Commands.BackgroundTidy
{
  public class ProjectOuputsWindow : Microsoft.VisualStudio.PlatformUI.DialogWindow
  {
    public ProjectOuputsWindow()
      : this(string.Empty)
    {
    }

    public ProjectOuputsWindow(string helpTopic)
      : base(helpTopic)
    {
      KeyUp += new System.Windows.Input.KeyEventHandler(DialogWindow_KeyUp);
    }

    private void DialogWindow_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
    {
      if (e.Key == System.Windows.Input.Key.Escape)
      {
        DialogResult = false;
        Close();
      }
    }

    protected override void InvokeDialogHelp()
    {
    }
  }
}
