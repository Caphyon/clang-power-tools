using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ClangPowerTools.MVVM.Views.Components
{
  /// <summary>
  /// Interaction logic for LicenseTypeButton.xaml
  /// </summary>
  public partial class LicenseTypeButton : UserControl
  {
    public LicenseTypeButton()
    {
      InitializeComponent();
    }

    public string Image
    {
      get { return (string)GetValue(ImageProperty); }
      set { SetValue(ImageProperty, value); }
    }

    public static readonly DependencyProperty ImageProperty =
      DependencyProperty.Register("Image", typeof(string), typeof(LicenseTypeButton), new PropertyMetadata(null));


    public string Title
    {
      get { return (string)GetValue(TitleProperty); }
      set { SetValue(TitleProperty, value); }
    }

    public static readonly DependencyProperty TitleProperty =
      DependencyProperty.Register("Title", typeof(string), typeof(LicenseTypeButton), new PropertyMetadata(null));

    public string Description
    {
      get { return (string)GetValue(DescriptionProperty); }
      set { SetValue(DescriptionProperty, value); }
    }

    public static readonly DependencyProperty DescriptionProperty =
      DependencyProperty.Register("Description", typeof(string), typeof(LicenseTypeButton), new PropertyMetadata(null));

    public ICommand ButtonCommand
    {
      get { return (ICommand)GetValue(ButtonCommandProperty); }
      set { SetValue(ButtonCommandProperty, value); }
    }

    public static readonly DependencyProperty ButtonCommandProperty =
      DependencyProperty.Register("ButtonCommand", typeof(ICommand), typeof(LicenseTypeButton), new PropertyMetadata(null));


  }
}
