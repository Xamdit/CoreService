namespace Service.Core.Extensions;

public class SpritExtension
{
  public static List<int> split_int(string input, string separator, int take = 0)
  {
    var output = input.Split(separator).Select(int.Parse).ToList();
    return take == 0 ? output : output.Take(take).ToList();
  }

  public static List<string> split_string(string input, string separator, int take = 0)
  {
    var output = input.Split(separator).ToList();
    return take == 0 ? output : output.Take(take).ToList();
  }
}
