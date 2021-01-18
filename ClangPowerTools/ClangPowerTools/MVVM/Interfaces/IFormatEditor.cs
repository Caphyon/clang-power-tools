using ClangPowerTools.MVVM.Interfaces;
using System.Collections.Generic;
using System.Windows.Input;

namespace ClangPowerTools
{
  public interface IFormatEditor
  {
    IEnumerable<ToggleValues> BooleanComboboxValues { get; }
    ICommand CreateFormatFileCommand { get; }
    ICommand ImportFormatFileCommand { get; }
    ICommand ResetCommand { get; }
    List<IFormatOption> FormatOptions { get; set; }
    IFormatOption SelectedOption { get; set; }
    EditorStyles SelectedStyle { get; set; }

    void OpenMultipleInput(int index);
  }
}