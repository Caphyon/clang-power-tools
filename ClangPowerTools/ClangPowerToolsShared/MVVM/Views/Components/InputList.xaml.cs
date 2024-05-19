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

    public ICommand PickFilesCommand
    {
      get { return (ICommand)GetValue(PickFilesCommandProperty); }
      set { SetValue(PickFilesCommandProperty, value); }
    }

    public static readonly DependencyProperty PickFilesCommandProperty =
      DependencyProperty.Register("PickFilesCommand", typeof(ICommand), typeof(InputList), new PropertyMetadata(null));

    public ICommand PickFolderCommand
    {
      get { return (ICommand)GetValue(PickFolderCommandProperty); }
      set { SetValue(PickFolderCommandProperty, value); }
    }

    public static readonly DependencyProperty PickFolderCommandProperty =
      DependencyProperty.Register("PickFolderCommand", typeof(ICommand), typeof(InputList), new PropertyMetadata(null));

    public ObservableCollection<InputDataModel> Collection
    {
      get { return (ObservableCollection<InputDataModel>)GetValue(CollectionProperty); }
      set { SetValue(CollectionProperty, value); }
    }

    public static readonly DependencyProperty CollectionProperty =
      DependencyProperty.Register("Collection", typeof(ObservableCollection<InputDataModel>), typeof(InputList), null);

    #endregion


    //#region Routed Events

    //public static readonly RoutedEvent DeleteButtonClickEvent = EventManager.RegisterRoutedEvent(
    //  "DeleteButtonClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(InputList));

    //public event RoutedEventHandler DeleteButtonClick
    //{
    //  add { AddHandler(DeleteButtonClickEvent, value); }
    //  remove { RemoveHandler(DeleteButtonClickEvent, value); }
    //}

    //private void DeleteButton_Click(object sender, RoutedEventArgs e)
    //{
    //  var newEventArgs = new RoutedEventArgs(DeleteButtonClickEvent);
    //  RaiseEvent(newEventArgs);
    //}

    //#endregion

    #region Methods

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
      var elementIndex = GetElementIndex(sender as FrameworkElement);
      ((InputDataViewModel)DataContext).DeleteInput(elementIndex);
    }

    private int GetElementIndex(FrameworkElement frameworkElement)
    {
      var element = frameworkElement.DataContext;
      return CollectionItems.Items.IndexOf(element);
    }

    #endregion

  }
}
