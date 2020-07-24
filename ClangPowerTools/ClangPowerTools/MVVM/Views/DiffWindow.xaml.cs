using System;
using System.Windows;

namespace ClangPowerTools.MVVM.Views
{
  /// <summary>
  /// Interaction logic for DiffWindow.xaml
  /// </summary>
  public partial class DiffWindow : Window
  {
    public DiffWindow(string html, string formatOptionFile, Action exportFormatOptionFile)
    {
      InitializeComponent();
      DataContext = new DiffViewModel(this, html, formatOptionFile, exportFormatOptionFile);
    }
  }
}
