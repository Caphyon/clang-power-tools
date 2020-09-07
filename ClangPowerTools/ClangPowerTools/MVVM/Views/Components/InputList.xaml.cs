using ClangPowerTools.MVVM.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ClangPowerTools.MVVM.Views.Components
{
  /// <summary>
  /// Interaction logic for InputList.xaml
  /// </summary>
  public partial class InputList : UserControl
  {
    #region Constructor

    public InputList()
    {
      InitializeComponent();
    }

    #endregion


    #region Properties

    public string InputToAdd
    {
      get { return (string)GetValue(InputToAddProperty); }
      set { SetValue(InputToAddProperty, value); }
    }

    public static readonly DependencyProperty InputToAddProperty =
      DependencyProperty.Register("InputToAdd", typeof(string), typeof(InputList), new PropertyMetadata(null));


    public ICommand AddCommand
    {
      get { return (ICommand)GetValue(AddCommandProperty); }
      set { SetValue(AddCommandProperty, value); }
    }

    public static readonly DependencyProperty AddCommandProperty =
      DependencyProperty.Register("AddCommand", typeof(ICommand), typeof(InputList), new PropertyMetadata(null));


    public ObservableCollection<InputDataModel> Collection
    {
      get { return (ObservableCollection<InputDataModel>)GetValue(CollectionProperty); }
      set { SetValue(CollectionProperty, value); }
    }

    public static readonly DependencyProperty CollectionProperty =
      DependencyProperty.Register("Collection", typeof(ObservableCollection<InputDataModel>), typeof(InputList), new PropertyMetadata(null));


    #region Properties on data model

    public bool ReadOnlyItem
    {
      get { return (bool)GetValue(ReadOnlyProperty); }
      set { SetValue(ReadOnlyProperty, value); }
    }

    public static readonly DependencyProperty ReadOnlyProperty =
      DependencyProperty.Register("ReadOnlyItem", typeof(bool), typeof(InputList), new PropertyMetadata(null));


    public string Item
    {
      get { return (string)GetValue(ItemProperty); }
      set { SetValue(ItemProperty, value); }
    }

    public static readonly DependencyProperty ItemProperty =
      DependencyProperty.Register("Item", typeof(string), typeof(InputList), new PropertyMetadata(null));

    #endregion

    #endregion


    #region Methods

    private readonly InputDataViewModel inputDataViewModel = new InputDataViewModel();

    private void DeleteButton(object sender, RoutedEventArgs e)
    {
      var elementIndex = GetElementIndex(sender as FrameworkElement);
      inputDataViewModel.DeleteInput(elementIndex);
    }

    private int GetElementIndex(FrameworkElement frameworkElement)
    {
      var element = frameworkElement.DataContext;
      return InputsList.Items.IndexOf(element);
    }

    #endregion
  }
}
