using System;
using System.Collections.Generic;
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
    #region Members

    private readonly DiffViewModel diffViewModel;

    #endregion

    #region Constructor

    public DiffWindow(List<(FlowDocument, FlowDocument)> flowDocuments, List<string> fileNames, string optionsFile, Action CreateFormatFile)
    {
      InitializeComponent();
      diffViewModel = new DiffViewModel(this, flowDocuments, fileNames, optionsFile, CreateFormatFile);
      DataContext = diffViewModel;
    }

    #endregion

    #region Private Methods

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

    #endregion
  }
}
