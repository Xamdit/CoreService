using Microsoft.Data.SqlClient;
using Service.Entities;
using SqlKata.Execution;

namespace Service.Helpers.Database;

public static class KataHelper
{
  private static string _connectionString = "Server=167.99.67.115;User=root;Password=Q7Okn2l_pg9Vvlp;Database=crm;Convert Zero Datetime=True;TreatTinyAsBoolean=True";

  public static SqlKata.Query kata(this MyContext db, string tablename)
  {
    var connection = new SqlConnection(_connectionString);
    var compiler = new SqlKata.Compilers.MySqlCompiler();
    var _db = new QueryFactory(connection, compiler);
    var query = _db.Query(tablename);
    return query;
  }

  public static QueryFactory kata(this MyContext db)
  {
    var connection = new SqlConnection(_connectionString);
    var compiler = new SqlKata.Compilers.MySqlCompiler();
    var _db = new QueryFactory(connection, compiler);
    return _db;
  }

  public static bool is_reference_in_table(this QueryFactory db, string field, string table, int id)
  {
    // var connection = new SqlConnection(_connectionString);
    // var compiler = new SqlKata.Compilers.MySqlCompiler();
    // var db = new QueryFactory(connection, compiler);
    var query = db.Query(table).Where(field, id).SelectRaw("1");

    try
    {
      // connection.Open();
      var result = query.FirstOrDefault();
      return result != null;
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error checking reference: {ex.Message}");
      return false;
    }
    finally
    {
      // if (connection.State != System.Data.ConnectionState.Closed) connection.Close();
    }

    return false;
  }
}
