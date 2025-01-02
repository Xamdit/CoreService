using iTextSharp.text.pdf;
using Newtonsoft.Json;
using Service.Framework.Schemas;
using Task = System.Threading.Tasks.Task;

namespace Service.Framework.Library.DataSync;

public class DataSyncServiceTestUnit
{
  [Test]
  public async Task TestSync()
  {
    var gateway = new DataSyncService("https://api.xamdit.com", LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<DataSyncService>());
    var route = "/api/models";
    var email = "codelanding@gmal.com";
    var password = "password";
    var isStaff = true;
    var self = new MyInstance();
    var response = await self.library.datasync().Post<SuccessResponse<AcroFields.Item>>(route, new { email, password, remember = false, isStaff });
    Console.WriteLine(JsonConvert.SerializeObject(response));
  }
}
