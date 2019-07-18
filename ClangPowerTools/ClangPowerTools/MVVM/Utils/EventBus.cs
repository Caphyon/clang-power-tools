using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClangPowerTools.MVVM.Utils
{
  public static class EventBus
  {
    private static readonly Dictionary<object, Action> Mapping = new Dictionary<object, Action>();

    public static void Register(object message, Action callback)
    {
      if (!Mapping.ContainsKey(message))
      {
        Mapping.Add(message, callback);
      }
    }
    public static void Unregister(object message, Action callback)
    {
      if (Mapping.ContainsKey(message))
      {
        Mapping.Remove(message);
      }
    }

    public static void Notify(object message)
    {
      if (Mapping.ContainsKey(message))
      {
        //Action callback = Mapping[message]();
        //callback();
        Mapping[message]();
      }
    }
  }
}
