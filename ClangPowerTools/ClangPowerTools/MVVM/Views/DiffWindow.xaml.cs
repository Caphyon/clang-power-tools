using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace ClangPowerTools.MVVM.Views
{
  /// <summary>
  /// Interaction logic for DiffWindow.xaml
  /// </summary>
  public partial class DiffWindow : Window
  {
    public DiffWindow(FlowDocument diffInput, FlowDocument diffOutput, string formatOptionFile, Action exportFormatOptionFile)
    {
      InitializeComponent();
      DataContext = new DiffViewModel(this, diffInput, diffOutput, formatOptionFile, exportFormatOptionFile);
    }

    private void Diff_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
      if (e.VerticalChange == 0 && e.HorizontalChange == 0) return;
      if (sender == DiffInput)
      {
        DiffOutput.ScrollToVerticalOffset(e.VerticalOffset);
        DiffOutput.ScrollToHorizontalOffset(e.HorizontalOffset);
      }
      else
      {
        DiffInput.ScrollToVerticalOffset(e.VerticalOffset);
        DiffInput.ScrollToHorizontalOffset(e.HorizontalOffset);
      }
    }
  }
}
