using ClangPowerToolsShared.MVVM.Models.ToolWindowModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ClangPowerToolsShared.MVVM.AutoCompleteHistory
{
  public static class AutoCompleteBehavior
  {
    public static TextChangedEventHandler OnListUpdate = delegate { };
    public static TextChangedEventHandler onTextChanged = new TextChangedEventHandler(OnTextChanged);
    public static KeyEventHandler onKeyDown = new KeyEventHandler(OnPreviewKeyDown);
    public static List<AutoCompleteHistoryModel> AutocompleteResult = new();
    public static string MatchText { get; set; } 

    /// <summary>
    /// The collection to search for matches from.
    /// </summary>
    public static readonly DependencyProperty AutoCompleteItemsSource =
        DependencyProperty.RegisterAttached
        (
            "AutoCompleteItemsSource",
            typeof(IEnumerable<AutoCompleteHistoryModel>),
            typeof(AutoCompleteBehavior),
            new UIPropertyMetadata(null, OnAutoCompleteItemsSource)
        );
    /// <summary>
    /// Whether or not to ignore case when searching for matches.
    /// </summary>
    public static readonly DependencyProperty AutoCompleteStringComparison =
      DependencyProperty.RegisterAttached
      (
        "AutoCompleteStringComparison",
        typeof(StringComparison),
        typeof(AutoCompleteBehavior),
        new UIPropertyMetadata(StringComparison.Ordinal)
      );

    /// <summary>
    /// What string should indicate that we should start giving auto-completion suggestions.  For example: @
    /// If this is null or empty, auto-completion suggestions will begin at the beginning of the textbox's text.
    /// </summary>
    public static readonly DependencyProperty AutoCompleteIndicator =
            DependencyProperty.RegisterAttached
            (
                "AutoCompleteIndicator",
                typeof(String),
                typeof(AutoCompleteBehavior),
                new UIPropertyMetadata(String.Empty)
            );

    #region Items Source
    public static IEnumerable<AutoCompleteHistoryModel> GetAutoCompleteItemsSource(DependencyObject obj)
    {
      object objRtn = obj.GetValue(AutoCompleteItemsSource);
      if (objRtn is IEnumerable<AutoCompleteHistoryModel>)
        return (objRtn as IEnumerable<AutoCompleteHistoryModel>);

      return null;
    }

    public static void SetAutoCompleteItemsSource(DependencyObject obj, IEnumerable<String> value)
    {
      obj.SetValue(AutoCompleteItemsSource, value);
    }

    private static void OnAutoCompleteItemsSource(object sender, DependencyPropertyChangedEventArgs e)
    {
      TextBox tb = sender as TextBox;
      if (sender == null)
        return;

      //If we're being removed, remove the callbacks
      //Remove our old handler, regardless of if we have a new one.
      tb.TextChanged -= onTextChanged;
      tb.PreviewKeyDown -= onKeyDown;
      if (e.NewValue != null)
      {
        //New source.  Add the callbacks
        tb.TextChanged += onTextChanged;
        tb.PreviewKeyDown += onKeyDown;
      }
    }
    #endregion

    #region String Comparison
    public static StringComparison GetAutoCompleteStringComparison(DependencyObject obj)
    {
      return (StringComparison)obj.GetValue(AutoCompleteStringComparison);
    }

    public static void SetAutoCompleteStringComparison(DependencyObject obj, StringComparison value)
    {
      obj.SetValue(AutoCompleteStringComparison, value);
    }
    #endregion

    #region Indicator
    public static String GetAutoCompleteIndicator(DependencyObject obj)
    {
      return (String)obj.GetValue(AutoCompleteIndicator);
    }

    public static void SetAutoCompleteIndicator(DependencyObject obj, String value)
    {
      obj.SetValue(AutoCompleteIndicator, value);
    }
    #endregion

    /// <summary>
    /// Used for moving the caret to the end of the suggested auto-completion text.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    static void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Down)
      {
        ((UIElement)e.OriginalSource).MoveFocus
          (new TraversalRequest(FocusNavigationDirection.Down));
      }

      if (e.Key != Key.Enter && e.Key != Key.Tab)
        return;

      TextBox tb = e.OriginalSource as TextBox;
      if (tb == null)
        return;

      //If we pressed enter and if the selected text goes all the way to the end, move our caret position to the end
      if (tb.SelectionLength > 0 && (tb.SelectionStart + tb.SelectionLength == tb.Text.Length))
      {
        tb.SelectionStart = tb.CaretIndex = tb.Text.Length;
        tb.SelectionLength = 0;
      }
    }

    /// <summary>
    /// Search for auto-completion suggestions.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// 

    static void OnTextChanged(object sender, TextChangedEventArgs e)
    {
      MatchText = String.Empty;
      TextBox tb = e.OriginalSource as TextBox;
      if (sender == null)
        return;
      IEnumerable<AutoCompleteHistoryModel> values = GetAutoCompleteItemsSource(tb);
      //No reason to search if we don't have any values.
      if (values == null)
        return;

      if (String.IsNullOrEmpty(tb.Text))
      {
        AutocompleteResult = values.ToList();
        OnListUpdate?.Invoke(sender, e);
      }
      if
      (
          (from change in e.Changes where change.RemovedLength > 0 select change).Any() &&
          (from change in e.Changes where change.AddedLength > 0 select change).Any() == false
      )
        return;

      //No reason to search if there's nothing there.
      if (String.IsNullOrEmpty(tb.Text))
        return;
      
      MatchText = tb.Text;

      List<string> indicators = new List<string>() { " ", "(", ")", "," };
      int startIndex = 0; //Start from the beginning of the line.
      string matchingString = tb.Text;
      //If we have a trigger string, make sure that it has been typed before
      //giving auto-completion suggestions.
      string rememberIndicator = string.Empty;
      if (indicators != null && !String.IsNullOrEmpty(indicators.First()))
      {
        if (tb.Text.Length > 0)
        {
          List<int> indicatorsIndex = new();
          indicatorsIndex = indicators.Select(a => tb.Text.LastIndexOf(a)).ToList();
          int maxIndicatorIndex = indicatorsIndex.Max();
          if (maxIndicatorIndex == -1)
          {
            startIndex = 0;
          }
          startIndex = 1 + maxIndicatorIndex;
          matchingString = tb.Text.Substring(startIndex, (tb.Text.Length - startIndex));
        }
      }

      //If we don't have anything after the trigger string, return.
      if (String.IsNullOrEmpty(matchingString))
      {
        AutocompleteResult = values.ToList();
        OnListUpdate?.Invoke(sender, e);
        return;
      }

      Int32 textLength = matchingString.Length;

      StringComparison comparer = GetAutoCompleteStringComparison(tb);
      //Do search and changes here.
      AutoCompleteHistoryModel match =
      (
        from
          value
        in
        (
          from subvalue
          in values
          where subvalue != null && subvalue.Value.Length >= textLength
          select subvalue
        )
        where value.Value.Substring(0, textLength).Equals(matchingString, comparer)
        select new AutoCompleteHistoryModel
        {
          Value = value.Value.Substring(textLength, value.Value.Length - textLength),
          RememberAsFavorit = value.RememberAsFavorit,
          PinIconPath = value.PinIconPath,
          Visibility = value.Visibility,
          Id = value.Id
        }/*Only select the last part of the suggestion*/
      ).FirstOrDefault();

      AutocompleteResult =
        (
          from
            value
          in
          (
            from subvalue
            in values
            where subvalue != null && subvalue.Value.Length >= textLength
            select subvalue
          )
          where value.Value.Substring(0, textLength).Equals(matchingString, comparer)
          select new AutoCompleteHistoryModel
          {
            Value = value.Value.Substring(textLength, value.Value.Length - textLength),
            RememberAsFavorit = value.RememberAsFavorit,
            PinIconPath = value.PinIconPath,
            Visibility = value.Visibility,
            Id = value.Id
          }/*Only select the last part of the suggestion*/
      ).ToList();

      OnListUpdate?.Invoke(sender, e);

      //Nothing.  Leave 'em alone
      if (match is null || String.IsNullOrEmpty(match.Value))
        return;

      int matchStart = (startIndex + matchingString.Length);
      tb.TextChanged -= onTextChanged;
      tb.Text += match.Value;
      tb.CaretIndex = matchStart;
      tb.SelectionStart = matchStart;
      tb.SelectionLength = (tb.Text.Length - startIndex);
      tb.TextChanged += onTextChanged;
    }

  }
}
