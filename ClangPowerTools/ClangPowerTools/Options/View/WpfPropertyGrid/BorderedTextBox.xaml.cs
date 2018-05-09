using System.Windows;
using ClangPowerTools.Options.View;
using ClangPowerTools.Options.View.WpfPropertyGrid;

namespace Caphyon.AdvInstVSIntegration.ProjectEditor.View.WpfPropertyGrid
{
  /// <summary>
  /// Interaction logic for BorderedTextBox.xaml
  /// </summary>
  public partial class BorderedTextBox : TextBoxNotifaiableUserControl
  {
    public BorderedTextBox()
    {
      InitializeComponent();
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
