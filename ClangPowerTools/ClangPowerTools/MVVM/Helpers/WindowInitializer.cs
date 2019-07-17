using ClangPowerTools.MVVM.ViewModels;
using ClangPowerTools.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClangPowerTools.MVVM.Helpers
{
  class WindowInitializer
  {
    public static void ShowWindow(List<string> selectedFiles)
    {
      var encodingConverterViewModel = new EncodingConverterViewModel(selectedFiles);
      encodingConverterViewModel.LoadData();

      var EncodingConverterWindow = WindowManager.CreateElementWindow(encodingConverterViewModel, Resources.EncodingConverterWindowTitle, "ClangPowerTools.MVVM.Views.EncodingConverterControl");

      if (encodingConverterViewModel.CloseAction == null)
      {
        encodingConverterViewModel.CloseAction = () => EncodingConverterWindow.Close();
      }

      EncodingConverterWindow.ShowDialog();
    }
  }
}
