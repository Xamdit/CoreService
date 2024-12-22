using Service.Framework.Sessions;

namespace Service.Framework.Library.Net.Sessions;

public class TestUnit
{
  private MyInstance self = MyInstance.Instance;

  [Test]
  public void Test()
  {
    var id = Guid.NewGuid().ToString();
    var session = new Session(self, id);
    session.set("name", "John Doe");
    Assert.That(session.get("name"), Is.EqualTo("John Doe"));
    // session.delete("name");
    // Assert.IsNull(session.get("name"));
  }
}
