using Microsoft.Data.SqlClient;
using Service.Entities;
using SqlKata.Execution;

namespace Service.Helpers.Database;

public static class KataHelper
{
  private static string _connectionString = "Server=167.99.67.115;User=root;Password=Q7Okn2l_pg9Vvlp;Database=crm;Convert Zero Datetime=True;TreatTinyAsBoolean=True";

  public static SqlKata.Query kata(this MyContext helper, string tablename)
  {
    var connection = new SqlConnection(_connectionString);
    var compiler = new SqlKata.Compilers.MySqlCompiler();
    var db = new QueryFactory(connection, compiler);

    var query = db.Query(tablename);
    //   .Select("Id", "Name", "Email")
    //   .Where("IsActive", true)
    //   .OrderBy("Name");
    //
    // var results = query.Get();
    //
    // foreach (var user in results) Console.WriteLine($"{user.Id}: {user.Name} ({user.Email})");
    return query;
  }
}
