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
    public DiffWindow(FlowDocument diffInput, FlowDocument diffOutput, string formatOptionFile, Action exportFormatOptionFile)
    {
      InitializeComponent();
      DataContext = new DiffViewModel(this, diffInput, diffOutput, formatOptionFile, exportFormatOptionFile);
    }
  }
}
