namespace Service.Core.Engine;

public class HelperBaseExtension
{
  private static bool isInitialized = false;

  // public static (MyInstance self, MyContext db) getInstance()
  // {
  //   if (isInitialized) return (self, db);
  //   self = MyInstance.Instance;
  //   db = new MyContext();
  //   isInitialized = true;
  //   return (self, db);
  // }
}
