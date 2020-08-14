using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ClangPowerTools.MVVM.Views.Components
{
  /// <summary>
  /// Interaction logic for ThreePieceButton.xaml
  /// </summary>
  public partial class ThreePieceButton : UserControl
  {
    public ThreePieceButton()
    {
      InitializeComponent();
    }

    public string Image
    {
      get { return (string)GetValue(ImageProperty); }
      set { SetValue(ImageProperty, value); }
    }

    public static readonly DependencyProperty ImageProperty =
      DependencyProperty.Register("Image", typeof(string), typeof(ThreePieceButton), new PropertyMetadata(null));


    public string Title
    {
      get { return (string)GetValue(TitleProperty); }
      set { SetValue(TitleProperty, value); }
    }

    public static readonly DependencyProperty TitleProperty =
      DependencyProperty.Register("Title", typeof(string), typeof(ThreePieceButton), new PropertyMetadata(null));


    public string Description
    {
      get { return (string)GetValue(DescriptionProperty); }
      set { SetValue(DescriptionProperty, value); }
    }

    public static readonly DependencyProperty DescriptionProperty =
      DependencyProperty.Register("Description", typeof(string), typeof(ThreePieceButton), new PropertyMetadata(null));


    public ICommand ButtonCommand
    {
      get { return (ICommand)GetValue(ButtonCommandProperty); }
      set { SetValue(ButtonCommandProperty, value); }
    }

    public static readonly DependencyProperty ButtonCommandProperty =
      DependencyProperty.Register("ButtonCommand", typeof(ICommand), typeof(ThreePieceButton), new PropertyMetadata(null));


    public string BackgroundValue
    {
      get { return (string)GetValue(BackgroundValueProperty); }
      set { SetValue(BackgroundValueProperty, value); }
    }

    public static readonly DependencyProperty BackgroundValueProperty =
      DependencyProperty.Register("BackgroundValue", typeof(string), typeof(ThreePieceButton), new PropertyMetadata(null));


    public string OpacityValue
    {
      get { return (string)GetValue(OpacityValueProperty); }
      set { SetValue(OpacityValueProperty, value); }
    }

    public static readonly DependencyProperty OpacityValueProperty =
      DependencyProperty.Register("OpacityValue", typeof(string), typeof(ThreePieceButton), new PropertyMetadata(null));


    public string WidthValue
    {
      get { return (string)GetValue(WidthValueProperty); }
      set { SetValue(WidthValueProperty, value); }
    }

    public static readonly DependencyProperty WidthValueProperty =
      DependencyProperty.Register("WidthValue", typeof(string), typeof(ThreePieceButton), new PropertyMetadata(null));


    public string HeightValue
    {
      get { return (string)GetValue(HeightValueProperty); }
      set { SetValue(HeightValueProperty, value); }
    }

    public static readonly DependencyProperty HeightValueProperty =
      DependencyProperty.Register("HeightValue", typeof(string), typeof(ThreePieceButton), new PropertyMetadata(null));

  }
}
