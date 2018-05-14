using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Caphyon.AdvInstVSIntegration.ProjectEditor.View.WpfPropertyGrid
{
  /// <summary>
  /// Interaction logic for BorderedComboBox.xaml
  /// </summary>
  public partial class BorderedComboBox : UserControl
  {
    public BorderedComboBox()
    {
      InitializeComponent();
    }

    private void ComboBoxPreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
      MouseUpDownEventForwarding(sender, e, UIElement.MouseUpEvent);
    }

    private void ComboBoxPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
      MouseUpDownEventForwarding(sender, e, UIElement.MouseDownEvent);
    }

    private void MouseUpDownEventForwarding(object sender, MouseButtonEventArgs e, RoutedEvent evFwd)
    {
      ComboBox tb = sender as ComboBox;
      if (null != tb)
      {
        Border b = tb.Parent as Border;
        if (null != b)
        {
          MouseButtonEventArgs newEvArgs = new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, e.ChangedButton);
          newEvArgs.RoutedEvent = evFwd;
          b.RaiseEvent(newEvArgs);
        }
      }
    }
  }
}
