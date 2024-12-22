namespace Service.Core.Synchronus;

public class SyncTestUnit
{
  [Test]
  public async Task TestGetWhere()
  {
    var test = new SyncBuilder("https://api.xamdit.com");
    var result = await test.get_where("/users/health", new { }, 10);
    Console.WriteLine(result.Content);
  }

  [Test]
  public async Task TestPost()
  {
    var test = new SyncBuilder("https://api.xamdit.com");
    var result = await test.post("/users/health", new { });
    Console.WriteLine(result.Content);
  }
}
