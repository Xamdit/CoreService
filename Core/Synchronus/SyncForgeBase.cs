namespace Service.Core.Synchronus;

public abstract class SyncForgeBase
{
  protected Database db;
  public List<string> Fields { get; set; } = new();
  public List<string> Keys { get; set; } = new();
  public List<string> PrimaryKeys { get; set; } = new();
  public string DbCharSet { get; set; } = string.Empty;

  protected string _createDatabase = "CREATE DATABASE {0}";
  protected string _dropDatabase = "DROP DATABASE {0}";
  protected string _createTable = "{0} {1} ({2}\n)";
  protected string _createTableIf = "CREATE TABLE IF NOT EXISTS";
  protected bool _createTableKeys = false;
  protected string _dropTableIf = "DROP TABLE IF EXISTS";
  protected string _renameTable = "ALTER TABLE {0} RENAME TO {1};";
  protected bool _unsigned = true;
  protected string _null = string.Empty;
  protected string _default = " DEFAULT ";

  public SyncForgeBase(Database db)
  {
    this.db = db;
    Console.WriteLine("Database Forge Class Initialized");
  }

  public bool CreateDatabase(string dbName)
  {
    if (string.IsNullOrEmpty(_createDatabase)) return db.DbDebug ? db.DisplayError("db_unsupported_feature") : false;

    if (!db.Query(string.Format(_createDatabase, db.EscapeIdentifiers(dbName)))) return db.DbDebug ? db.DisplayError("db_unable_to_create") : false;

    db.DataCache["db_names"].Add(dbName);
    return true;
  }

  public bool DropDatabase(string dbName)
  {
    if (string.IsNullOrEmpty(_dropDatabase)) return db.DbDebug ? db.DisplayError("db_unsupported_feature") : false;

    if (!db.Query(string.Format(_dropDatabase, db.EscapeIdentifiers(dbName)))) return db.DbDebug ? db.DisplayError("db_unable_to_drop") : false;

    if (db.DataCache["db_names"].Contains(dbName)) db.DataCache["db_names"].Remove(dbName);

    return true;
  }

  public SyncForgeBase AddKey(string key, bool primary = false)
  {
    if (primary)
      PrimaryKeys.Add(key);
    else
      Keys.Add(key);

    return this;
  }

  public SyncForgeBase AddField(string field)
  {
    if (field == "id")
    {
      AddField(new Dictionary<string, object>
      {
        { "id", new { type = "INT", constraint = 9, auto_increment = true } }
      });
      AddKey("id", true);
    }
    else
    {
      if (!field.Contains(" ")) throw new Exception("Field information is required for that operation.");

      Fields.Add(field);
    }

    return this;
  }

  public SyncForgeBase AddField(Dictionary<string, object> field)
  {
    foreach (var item in field) Fields.Add($"{item.Key} {item.Value}");

    return this;
  }

  public bool CreateTable(string table, bool ifNotExists = false, Dictionary<string, string> attributes = null)
  {
    if (string.IsNullOrEmpty(table)) throw new Exception("A table name is required for that operation.");

    if (Fields.Count == 0) throw new Exception("Field information is required.");

    var sql = _createTableQuery(table, ifNotExists, attributes);

    if (string.IsNullOrEmpty(sql))
    {
      _reset();
      return db.DbDebug ? db.DisplayError("db_unsupported_feature") : false;
    }

    var result = db.Query(sql);

    if (result)
    {
      db.DataCache["table_names"].Add(table);

      if (Keys.Count > 0)
      {
        var indexQueries = _processIndexes(table);
        foreach (var query in indexQueries) db.Query(query);
      }
    }

    _reset();
    return result;
  }

  protected string _createTableQuery(string table, bool ifNotExists, Dictionary<string, string> attributes)
  {
    if (ifNotExists && _createTableIf == null)
    {
      if (db.TableExists(table)) return null;

      ifNotExists = false;
    }

    var sql = ifNotExists ? string.Format(_createTableIf, db.EscapeIdentifiers(table)) : "CREATE TABLE";
    var columns = _processFields();

    return string.Format(_createTable, sql, db.EscapeIdentifiers(table), columns);
  }

  protected string _processFields()
  {
    return string.Join(", ", Fields);
  }

  protected List<string> _processIndexes(string table)
  {
    var indexes = new List<string>();

    foreach (var key in Keys) indexes.Add($"ALTER TABLE {db.EscapeIdentifiers(table)} ADD INDEX ({db.EscapeIdentifiers(key)});");

    return indexes;
  }

  public bool DropTable(string tableName, bool ifExists = false)
  {
    if (string.IsNullOrEmpty(tableName)) return db.DbDebug ? db.DisplayError("db_table_name_required") : false;

    var sql = _dropTableQuery(tableName, ifExists);
    var result = db.Query(sql);

    if (result) db.DataCache["table_names"].Remove(tableName);

    return result;
  }

  protected string _dropTableQuery(string tableName, bool ifExists)
  {
    var sql = "DROP TABLE";

    if (ifExists && !db.TableExists(tableName)) return null;

    return $"{sql} {db.EscapeIdentifiers(tableName)}";
  }

  private void _reset()
  {
    Fields.Clear();
    Keys.Clear();
    PrimaryKeys.Clear();
  }
}

public class Database
{
  public bool DbDebug { get; set; }
  public Dictionary<string, List<string>> DataCache { get; set; } = new();

  public string EscapeIdentifiers(string identifier)
  {
    // Implement identifier escaping based on your database requirements
    return identifier;
  }

  public bool Query(string query)
  {
    // Implement your query execution logic
    return true;
  }

  public bool TableExists(string tableName)
  {
    // Implement table existence check
    return true;
  }

  public bool DisplayError(string message)
  {
    // Handle error display
    Console.WriteLine($"Error: {message}");
    return false;
  }
}
