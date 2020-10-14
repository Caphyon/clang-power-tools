using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ClangPowerTools.MVVM.Views.Components
{
  /// <summary>
  /// Interaction logic for ThreePieceComponent.xaml
  /// </summary>
  public partial class ThreePieceComponent : UserControl
  {
    public ThreePieceComponent()
    {
      InitializeComponent();
    }

    #region Component

    public string BackgroundValue
    {
      get { return (string)GetValue(BackgroundValueProperty); }
      set { SetValue(BackgroundValueProperty, value); }
    }

    public static readonly DependencyProperty BackgroundValueProperty =
      DependencyProperty.Register("BackgroundValue", typeof(string), typeof(ThreePieceComponent), new PropertyMetadata(null));


    public string OpacityValue
    {
      get { return (string)GetValue(OpacityValueProperty); }
      set { SetValue(OpacityValueProperty, value); }
    }

    public static readonly DependencyProperty OpacityValueProperty =
      DependencyProperty.Register("OpacityValue", typeof(string), typeof(ThreePieceComponent), new PropertyMetadata(null));


    public string WidthValue
    {
      get { return (string)GetValue(WidthValueProperty); }
      set { SetValue(WidthValueProperty, value); }
    }

    public static readonly DependencyProperty WidthValueProperty =
      DependencyProperty.Register("WidthValue", typeof(string), typeof(ThreePieceComponent), new PropertyMetadata(null));


    public string HeightValue
    {
      get { return (string)GetValue(HeightValueProperty); }
      set { SetValue(HeightValueProperty, value); }
    }

    public static readonly DependencyProperty HeightValueProperty =
      DependencyProperty.Register("HeightValue", typeof(string), typeof(ThreePieceComponent), new PropertyMetadata(null));

    #endregion


    #region ImageComponent

    public string Image
    {
      get { return (string)GetValue(ImageProperty); }
      set { SetValue(ImageProperty, value); }
    }

    public static readonly DependencyProperty ImageProperty =
      DependencyProperty.Register("Image", typeof(string), typeof(ThreePieceComponent), new PropertyMetadata(null));


    public string ImageWidthValue
    {
      get { return (string)GetValue(ImageWidthValueProperty); }
      set { SetValue(ImageWidthValueProperty, value); }
    }

    public static readonly DependencyProperty ImageWidthValueProperty =
      DependencyProperty.Register("ImageWidthValue", typeof(string), typeof(ThreePieceComponent), new PropertyMetadata(null));


    public string ImageHeightValue
    {
      get { return (string)GetValue(ImageHeightValueProperty); }
      set { SetValue(ImageHeightValueProperty, value); }
    }

    public static readonly DependencyProperty ImageHeightValueProperty =
      DependencyProperty.Register("ImageHeightValue", typeof(string), typeof(ThreePieceComponent), new PropertyMetadata(null));


    #endregion


    #region TitleComponent


    public string Title
    {
      get { return (string)GetValue(TitleProperty); }
      set { SetValue(TitleProperty, value); }
    }

    public static readonly DependencyProperty TitleProperty =
      DependencyProperty.Register("Title", typeof(string), typeof(ThreePieceComponent), new PropertyMetadata(null));


    public string TitleFontSize
    {
      get { return (string)GetValue(TitleFontSizeProperty); }
      set { SetValue(TitleFontSizeProperty, value); }
    }

    public static readonly DependencyProperty TitleFontSizeProperty =
      DependencyProperty.Register("TitleFontSize", typeof(string), typeof(ThreePieceComponent), new PropertyMetadata(null));


    public string TitleForeground
    {
      get { return (string)GetValue(TitleForegroundProperty); }
      set { SetValue(TitleForegroundProperty, value); }
    }

    public static readonly DependencyProperty TitleForegroundProperty =
      DependencyProperty.Register("TitleForeground", typeof(string), typeof(ThreePieceComponent), new PropertyMetadata(null));


    #endregion


    #region DescriptionComponent

    public string Description
    {
      get { return (string)GetValue(DescriptionProperty); }
      set { SetValue(DescriptionProperty, value); }
    }

    public static readonly DependencyProperty DescriptionProperty =
      DependencyProperty.Register("Description", typeof(string), typeof(ThreePieceComponent), new PropertyMetadata(null));


    public string DescriptionFontSize
    {
      get { return (string)GetValue(DescriptionFontSizeProperty); }
      set { SetValue(DescriptionFontSizeProperty, value); }
    }

    public static readonly DependencyProperty DescriptionFontSizeProperty =
      DependencyProperty.Register("DescriptionFontSize", typeof(string), typeof(ThreePieceComponent), new PropertyMetadata(null));


    public string DescriptionForeground
    {
      get { return (string)GetValue(DescriptionForegroundProperty); }
      set { SetValue(DescriptionForegroundProperty, value); }
    }

    public static readonly DependencyProperty DescriptionForegroundProperty =
      DependencyProperty.Register("DescriptionForeground", typeof(string), typeof(ThreePieceComponent), new PropertyMetadata(null));

    #endregion


    #region Commands

    public ICommand ButtonCommand
    {
      get { return (ICommand)GetValue(ButtonCommandProperty); }
      set { SetValue(ButtonCommandProperty, value); }
    }

    public static readonly DependencyProperty ButtonCommandProperty =
      DependencyProperty.Register("ButtonCommand", typeof(ICommand), typeof(ThreePieceComponent), new PropertyMetadata(null));

    #endregion

  }
}
