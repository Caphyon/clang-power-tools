using ClangPowerTools.MVVM.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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

    public DiffWindow(List<IFormatOption> formatOptions, EditorStyles editorStyle, string editorInput, List<string> filePaths, Action exportFormatOptionFile)
    {
      InitializeComponent();
      diffViewModel = new DiffViewModel(this, formatOptions, editorStyle, editorInput, filePaths, exportFormatOptionFile);
      DataContext = diffViewModel;
    }

    #endregion

    #region Public Methods

    public async Task ShowDiffAsync()
    {
      await diffViewModel.ShowDiffAsync();
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
