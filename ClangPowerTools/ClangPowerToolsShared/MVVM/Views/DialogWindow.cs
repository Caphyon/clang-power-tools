namespace ClangPowerTools.MVVM.Views
{
  public class DialogWindow : Microsoft.VisualStudio.PlatformUI.DialogWindow
  {
    public DialogWindow()
      : this(string.Empty)
    {
    }

    public DialogWindow(string helpTopic)
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
