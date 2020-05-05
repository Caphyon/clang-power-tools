using ClangPowerTools.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClangPowerTools.MVVM
{
  public static class CollectionElementsCounter
  {
    #region Members

    private static int count = 0;
    private static int length = 0;
    private static bool full = false;

    public static event EventHandler<BoolEventArgs> ButtonStateEvent;

    public static bool Skip { get; set; }

    #endregion


    #region Methods

    public static void Initialize(IEnumerable<TidyCheckModel> collection)
    {
      Skip = false;
      count = 0;
      length = collection.Count();
      foreach (var item in collection)
      {
        if (item.IsChecked)
          ++count;
      }
      full = count == length;
    }

    public static void Add()
    {
      ++count;
      if (count == length)
      {
        full = true;
        ButtonStateEvent?.Invoke(null, new BoolEventArgs(true));
      }
    }

    public static void Remove()
    {
      --count;
      if (full)
      {
        full = false;
        ButtonStateEvent?.Invoke(null, new BoolEventArgs(false));
      }
    }

    public static bool IsEmpty() => length == 0;

    #endregion

  }
}
