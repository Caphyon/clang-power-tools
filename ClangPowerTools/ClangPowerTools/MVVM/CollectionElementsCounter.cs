using ClangPowerTools.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClangPowerTools.MVVM
{
  /// <summary>
  /// Count certain elements from a given collection
  /// </summary>
  public static class CollectionElementsCounter
  {
    #region Members

    /// <summary>
    /// Count the wanted collection elements
    /// </summary>
    private static int count = 0;

    /// <summary>
    /// Collection length
    /// </summary>
    private static int length = 0;

    /// <summary>
    /// Flag for counting all the collection elements 
    /// </summary>
    private static bool full = false;

    /// <summary>
    /// Event triggered when certain conditions are validated
    /// </summary>
    public static event EventHandler<BoolEventArgs> StateEvent;

    #endregion


    #region Methods

    /// <summary>
    /// Count collection elements and set the values for the other members
    /// </summary>
    /// <param name="collection">The collection under surveillance</param>
    public static void Initialize(IEnumerable<TidyCheckModel> collection)
    {
      count = 0;
      length = collection.Count();
      foreach (var item in collection)
      {
        if (item.IsChecked)
          ++count;
      }
      full = count == length;
    }

    /// <summary>
    /// Increment the counter. Trigger the StateEvent if the counter riched the maximum value.
    /// </summary>
    public static void Add()
    {
      ++count;
      if (count == length)
      {
        full = true;
        StateEvent?.Invoke(null, new BoolEventArgs(true));
      }
    }

    /// <summary>
    /// Decrement the counter. Trigger the StateEvent if in the previous state the counter riched the maximum value.
    /// </summary>
    public static void Remove()
    {
      --count;
      if (full)
      {
        full = false;
        StateEvent?.Invoke(null, new BoolEventArgs(false));
      }
    }

    /// <summary>
    /// Check if the collection is empty.
    /// </summary>
    /// <returns>True is the collection is empty. False otherwise</returns>
    public static bool IsEmpty() => length == 0;

    #endregion

  }
}
