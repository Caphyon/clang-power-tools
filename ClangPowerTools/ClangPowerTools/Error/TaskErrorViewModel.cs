using System.Collections.Generic;

namespace ClangPowerTools.Error
{
  public static class TaskErrorViewModel
  {
    public static List<TaskErrorModel> Errors { get; set; } = new List<TaskErrorModel>();

    public static Dictionary<string, List<TaskErrorModel>> FileErrorsPair { get; set; } = new Dictionary<string, List<TaskErrorModel>>();

  }
}
