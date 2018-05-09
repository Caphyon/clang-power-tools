using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ClangPowerTools.Options.View
{
  public class TextBoxNotifaiableUserControl : UserControl, INotifyPropertyChanged
  {
    public TextBoxNotifaiableUserControl()
    {
    }

    protected void TextBoxPreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
      MouseUpDownEventForwarding(sender, e, UIElement.MouseUpEvent);
    }

    protected void MouseUpDownEventForwarding(object sender, MouseButtonEventArgs e, RoutedEvent evFwd)
    {
      TextBox tb = sender as TextBox;
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

    protected void TextBoxPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
      MouseUpDownEventForwarding(sender, e, UIElement.MouseDownEvent);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public void NotifyPropertyChanged(string propName)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(propName));
    }
 
  }
}
