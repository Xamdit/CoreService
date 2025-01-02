using Service.Framework.Core.Engine;

namespace Service.Framework.Core.Extensions;

public static class ArrayExtension
{
  public static bool is_array(this HelperBase helper, object? obj)
  {
    return obj != null && obj.GetType().IsArray;
  }
}
