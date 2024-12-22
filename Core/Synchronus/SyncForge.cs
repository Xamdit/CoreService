using System.Text;

namespace Service.Core.Synchronus;

public class SyncForge
{
  private const string CreateDatabaseCommand = "CREATE DATABASE {0} CHARACTER SET {1} COLLATE {2}";
  private bool CreateTableKeys = true;

  private readonly List<string> UnsignedTypes = new()
  {
    "TINYINT", "SMALLINT", "MEDIUMINT", "INT", "INTEGER", "BIGINT", "REAL", "DOUBLE", "DOUBLE PRECISION", "FLOAT", "DECIMAL", "NUMERIC"
  };

  // private string NullValue = "NULL";
  private dynamic Db; // Represent the database object

  public SyncForge(dynamic db)
  {
    Db = db;
  }

  protected string CreateTableAttributes(Dictionary<string, string> attributes)
  {
    var sql = new StringBuilder();

    foreach (var key in attributes.Keys.Where(key => !string.IsNullOrEmpty(key)))
      sql.AppendFormat(" {0} = {1}", key.ToUpper(), attributes[key]);

    if (!string.IsNullOrEmpty(Db.char_set) && !sql.ToString().Contains("CHARACTER SET") && !sql.ToString().Contains("CHARSET")) sql.AppendFormat(" DEFAULT CHARACTER SET = {0}", Db.char_set);

    if (!string.IsNullOrEmpty(Db.dbcollat) && !sql.ToString().Contains("COLLATE")) sql.AppendFormat(" COLLATE = {0}", Db.dbcollat);

    return sql.ToString();
  }

  protected string AlterTable(string alterType, string table, List<Dictionary<string, dynamic>> fields)
  {
    if (alterType == "DROP") return $"ALTER TABLE {Db.EscapeIdentifiers(table)} DROP COLUMN {fields[0]["name"]};";

    var sql = new StringBuilder($"ALTER TABLE {Db.EscapeIdentifiers(table)}");
    for (var i = 0; i < fields.Count; i++)
    {
      var field = fields[i];
      if (field["_literal"] != null && field["_literal"] != false)
      {
        sql.AppendFormat(alterType == "ADD" ? "\n\tADD {0}" : "\n\tMODIFY {0}", field["_literal"]);
      }
      else
      {
        var literal = alterType == "ADD" ? "\n\tADD " : field.ContainsKey("new_name") && !string.IsNullOrEmpty(field["new_name"]) ? "\n\tCHANGE " : "\n\tMODIFY ";
        sql.Append(literal + ProcessColumn(field));
      }

      if (i < fields.Count - 1) sql.Append(", ");
    }

    return sql.ToString();
  }

  protected string ProcessColumn(Dictionary<string, dynamic> field)
  {
    string extraClause = field.ContainsKey("after") ? " AFTER " + Db.EscapeIdentifiers(field["after"]) : string.Empty;

    if (string.IsNullOrEmpty(extraClause) && field.ContainsKey("first") && field["first"] == true) extraClause = " FIRST";

    return $"{Db.EscapeIdentifiers(field["name"])}"
           + (field.ContainsKey("new_name") ? " " + Db.EscapeIdentifiers(field["new_name"]) : string.Empty)
           + $" {field["type"]}{field["length"]}{field["unsigned"]}{field["null"]}{field["default"]}{field["auto_increment"]}{field["unique"]}"
           + (!string.IsNullOrEmpty(field["comment"]) ? $" COMMENT {field["comment"]}" : string.Empty)
           + extraClause;
  }

  protected string ProcessIndexes(string table)
  {
    var sql = new StringBuilder();

    foreach (var key in Db.keys)
      if (key is List<string> keyList)
      {
        for (var i = keyList.Count - 1; i >= 0; i--)
          if (!Db.fields.ContainsKey(keyList[i]))
            keyList.RemoveAt(i);
      }
      else if (!Db.fields.ContainsKey(key))
      {
        continue;
      }

    // if (!(key is List<string>))
    // {
    //   key = new List<string> { key };
    // }
    // sql.AppendFormat(",\n\tKEY {0} ({1})", Db.EscapeIdentifiers(string.Join("_", key)), string.Join(", ", key.Select(k => Db.EscapeIdentifiers(k))));
    Db.keys.Clear();

    return sql.ToString();
  }
}
