using System.Windows;
using System.Windows.Controls;

namespace ClangPowerTools.MVVM.Views.Components
{
  /// <summary>
  /// Interaction logic for MessageBanner.xaml
  /// </summary>
  public partial class MessageBanner : UserControl
  {
    public MessageBanner()
    {
      InitializeComponent();
    }

    public string Banner
    {
      get { return (string)GetValue(BannerProperty); }
      set { SetValue(BannerProperty, value); }
    }

    public static readonly DependencyProperty BannerProperty =
      DependencyProperty.Register("Banner", typeof(string), typeof(MessageBanner), new PropertyMetadata(null));


    public string Icon
    {
      get { return (string)GetValue(IconProperty); }
      set { SetValue(IconProperty, value); }
    }

    public static readonly DependencyProperty IconProperty =
      DependencyProperty.Register("Icon", typeof(string), typeof(MessageBanner), new PropertyMetadata(null));


    public string Type
    {
      get { return (string)GetValue(TypeProperty); }
      set { SetValue(TypeProperty, value); }
    }

    public static readonly DependencyProperty TypeProperty =
      DependencyProperty.Register("Type", typeof(string), typeof(MessageBanner), new PropertyMetadata(null));

  }
}
