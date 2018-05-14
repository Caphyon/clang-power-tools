using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ClangPowerTools.Options.View.WpfPropertyGrid;

namespace Caphyon.AdvInstVSIntegration.ProjectEditor.View.WpfPropertyGrid
{
  /// <summary>
  /// Interaction logic for BorderedTextBox.xaml
  /// </summary>
  public partial class BorderedTextBox : UserControl, INotifyPropertyChanged
  {
    public BorderedTextBox()
    {
      InitializeComponent();
    }

    private void TextBoxPreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
      MouseUpDownEventForwarding(sender, e, UIElement.MouseUpEvent);
    }

    private void MouseUpDownEventForwarding(object sender, MouseButtonEventArgs e, RoutedEvent evFwd)
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

    private void TextBoxPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
      MouseUpDownEventForwarding(sender, e, UIElement.MouseDownEvent);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public void NotifyPropertyChanged(string propName)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(propName));
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
      PropertyData propData = (PropertyData)DataContext;

      StringCollectionEditor stringCollectionEditor = new StringCollectionEditor((string)propData.Value);
      if (stringCollectionEditor.ShowDialog() == true)
        propData.Value = stringCollectionEditor.CollectionValue;
    }

  }
}
