using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ClangPowerTools.MVVM.Helpers
{
  class WindowManager
  {
    public static System.Windows.Window CreateElementWindow(object viewModel, string title, string controlPath)
    {
      var window = new Window();
      window.Title = title;
      window.Background = Brushes.White;
      window.Foreground = Brushes.Black;
      window.SizeToContent = SizeToContent.Height;
      window.Width = 800;
      window.ResizeMode = ResizeMode.NoResize;
      window.WindowStartupLocation = WindowStartupLocation.CenterScreen;

      var controlAssembly = Assembly.Load("ClangPowerTools");
      var controlType = controlAssembly.GetType(controlPath);
      var newControl = Activator.CreateInstance(controlType) as UserControl;
      newControl.DataContext = viewModel;
      window.Content = newControl;

      return window;
    }
  }
}
