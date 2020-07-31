using System;
using System.Windows;
using System.Windows.Documents;

namespace ClangPowerTools.MVVM.Views
{
  /// <summary>
  /// Interaction logic for DiffWindow.xaml
  /// </summary>
  public partial class DiffWindow : Window
  {
    public DiffWindow(FlowDocument diffText, string formatOptionFile, Action exportFormatOptionFile)
    {
      InitializeComponent();
      DataContext = new DiffViewModel(this, diffText, formatOptionFile, exportFormatOptionFile);
    }
  }
}
