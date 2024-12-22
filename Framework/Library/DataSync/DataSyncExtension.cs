using Newtonsoft.Json;
using Service.Framework.Core.Engine;

namespace Service.Framework.Library.DataSync;

public static class DataSyncExtension
{
  public static DataSyncService datasync(this LibraryBase libraryBase)
  {
    // var self = libraryBase.parent;
    // var gateway_url = self.config["gateway_url"];
    var file = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
    var jsonString = File.ReadAllText(file);
    var json = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonString);
    var gateway_url = json["gateway_url"];
    var gateway = new DataSyncService(gateway_url, LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<DataSyncService>());
    return gateway;
  }
}
