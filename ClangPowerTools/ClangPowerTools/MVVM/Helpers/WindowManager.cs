using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ClangPowerTools.MVVM.Helpers
{
  class WindowManager
  {
    public static Window CreateElementWindow(object viewModel, string title, string controlPath)
    {
      var window = new Window();
      window.Title = title;
      window.SizeToContent = SizeToContent.WidthAndHeight;
      //window.ResizeMode = ResizeMode.NoResize;
      //window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
      window.Icon = BitmapFrame.Create(new Uri("pack://application:,,,/ClangPowerTools;component/Resources/ClangPowerToolsIco.ico", UriKind.RelativeOrAbsolute));

      var controlAssembly = Assembly.Load("ClangPowerTools");
      var controlType = controlAssembly.GetType(controlPath);
      var newControl = Activator.CreateInstance(controlType) as UserControl;
      newControl.DataContext = viewModel;
      window.Content = newControl;

      return window;
    }
  }
}
