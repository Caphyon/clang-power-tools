using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClangPowerTools.MVVM.Utils
{
  public static class EventBus
  {
    private static readonly Dictionary<object, List<Action>> Mapping = new Dictionary<object, List<Action>>();

    public static void Register(object message, Action callback)
    {
      if (Mapping.ContainsKey(message))
      {
        //Mapping[message].Add(callback);
      }
      else
      {
        List<Action> callbacks = new List<Action>();
        Mapping.Add(message, callbacks);
        callbacks.Add(callback);
      }
    }
    public static void Unregister(object message, Action callback)
    {
      if (Mapping.ContainsKey(message))
      {
        Mapping[message].Remove(callback);
      }
    }

    public static void Notify(object message)
    {
      if (Mapping.ContainsKey(message))
      {
        Mapping[message].ForEach(callback => callback());
      }
    }
  }
}
