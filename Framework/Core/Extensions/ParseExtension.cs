using DotLiquid;

namespace Service.Framework.Core.Extensions;

public static class ParseExtension
{
  public static string parse(this MyInstance self, string path, object data = default)
  {
    var content = File.ReadAllText(path);
    var template = Template.Parse(content);
    var output = template.Render(Hash.FromAnonymousObject(data));
    return output;
  }
}
