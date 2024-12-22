using System.Text;

namespace Service.Core.Synchronus;

public class SyncUtility(dynamic db)
{
  private const string ListDatabasesCommand = "SHOW DATABASES";
  private const string OptimizeTableCommand = "OPTIMIZE TABLE {0}";
  private const string RepairTableCommand = "REPAIR TABLE {0}";
  private dynamic Db; // Represent the database object


  public string Backup(Dictionary<string, dynamic> parameters)
  {
    if (parameters.Count == 0) return null; // No parameters provided

    bool addDrop = parameters.ContainsKey("add_drop") ? parameters["add_drop"] : true;
    bool addInsert = parameters.ContainsKey("add_insert") ? parameters["add_insert"] : true;
    bool foreignKeyChecks = parameters.ContainsKey("foreign_key_checks") ? parameters["foreign_key_checks"] : true;
    var tables = parameters.ContainsKey("tables") ? (string[])parameters["tables"] : Array.Empty<string>();
    var ignoreTables = parameters.ContainsKey("ignore") ? (string[])parameters["ignore"] : Array.Empty<string>();
    string newline = parameters.ContainsKey("newline") ? parameters["newline"] : Environment.NewLine;

    var output = new StringBuilder();

    if (!foreignKeyChecks) output.Append($"SET foreign_key_checks = 0;{newline}");

    foreach (var table in tables)
    {
      if (ignoreTables.ToList().Contains(table)) continue;

      var createTableQuery = $"SHOW CREATE TABLE {Db.EscapeIdentifiers(Db.database + "." + table)}";
      var queryResult = Db.Query(createTableQuery);
      if (queryResult == null || queryResult.Rows.Count == 0) continue;

      output.AppendFormat("#{0}# TABLE STRUCTURE FOR: {1}{0}#{0}{0}", newline, table);

      if (addDrop) output.AppendFormat("DROP TABLE IF EXISTS {0};{1}{1}", Db.ProtectIdentifiers(table), newline);

      string createTableSql = queryResult.Rows[0][1].ToString();
      output.AppendLine(createTableSql + ";" + newline);

      if (!addInsert) continue;

      var selectAllQuery = $"SELECT * FROM {Db.ProtectIdentifiers(table)}";
      var dataQueryResult = Db.Query(selectAllQuery);

      if (dataQueryResult.Rows.Count == 0) continue;

      var fieldStr = new StringBuilder();
      var isInt = new bool[dataQueryResult.Columns.Count];

      for (var i = 0; i < dataQueryResult.Columns.Count; i++)
      {
        string fieldType = dataQueryResult.Columns[i].DataType.Name.ToLower();
        isInt[i] = new[] { "tinyint", "smallint", "mediumint", "int", "bigint" }.Contains(fieldType);
        fieldStr.Append(Db.EscapeIdentifiers(dataQueryResult.Columns[i].ColumnName) + ", ");
      }

      var fieldStrTrimmed = fieldStr.ToString().TrimEnd(' ', ',');

      foreach (var row in dataQueryResult.Rows)
      {
        var valStr = new StringBuilder();

        for (var i = 0; i < dataQueryResult.Columns.Count; i++)
        {
          var value = row[i];
          if (value == DBNull.Value)
            valStr.Append("NULL");
          else
            valStr.Append(isInt[i] ? value.ToString() : Db.Escape(value.ToString()));
          valStr.Append(", ");
        }

        var valStrTrimmed = valStr.ToString().TrimEnd(' ', ',');
        output.AppendFormat("INSERT INTO {0} ({1}) VALUES ({2});{3}", Db.ProtectIdentifiers(table), fieldStrTrimmed, valStrTrimmed, newline);
      }

      output.AppendLine(newline);
    }

    if (!foreignKeyChecks) output.Append($"SET foreign_key_checks = 1;{newline}");

    return output.ToString();
  }
}
