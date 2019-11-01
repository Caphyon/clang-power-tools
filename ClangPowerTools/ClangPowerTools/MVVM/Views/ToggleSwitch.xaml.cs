using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClangPowerTools.MVVM.Views
{
  /// <summary>
  /// Interaction logic for ToggleSwitch.xaml
  /// </summary>
  public partial class ToggleSwitch : UserControl
  {
    Thickness leftSide = new Thickness(-39, 0, 0, 0);

    Thickness rightSide = new Thickness(0, 0, -39, 0);

    SolidColorBrush off = new SolidColorBrush(Color.FromRgb(160, 160, 160));

    SolidColorBrush on = new SolidColorBrush(Color.FromRgb(158, 0, 90));

    private bool toggled = false;

    public ToggleSwitch()
    {
      InitializeComponent();

      Back.Fill = off;
      toggled = false;
      Dot.Margin = leftSide;

    }

    public bool Toggled { get => toggled; set => toggled = value; }

    private void Dot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      if (toggled == false)
      {
        Back.Fill = on;
        toggled = true;
        Dot.Margin = rightSide;
      }
      else
      {
        Back.Fill = off;
        toggled = false;
        Dot.Margin = leftSide;
      }
    }

    private void Back_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      if (toggled == false)
      {
        Back.Fill = on;
        toggled = true;
        Dot.Margin = rightSide;
      }
      else
      {
        Back.Fill = off;
        toggled = false;
        Dot.Margin = leftSide;
      }
    }
  }
}
