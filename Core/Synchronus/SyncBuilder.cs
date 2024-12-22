using Newtonsoft.Json;

namespace Service.Core.Synchronus;

public class SyncBuilder(string api) : SyncDriver(api)
{
  public string gateway = api;
  protected List<string> qb_select = new();
  protected bool qb_distinct = false;
  protected List<string> qb_from = new();
  protected List<string> qb_join = new();
  protected List<string> qb_where = new();
  protected List<string> qb_groupby = new();
  protected List<string> qb_having = new();
  protected List<string> qb_keys = new();
  protected bool qb_limit = false;
  protected bool qb_offset = false;
  protected List<string> qb_orderby = new();
  protected List<string> qb_set = new();
  protected List<string> qb_set_ub = new();
  protected List<string> qb_aliased_tables = new();
  protected bool qb_where_group_started = false;
  protected int qb_where_group_count = 0;
  protected bool qb_caching = false;
  protected List<string> qb_cache_exists = new();
  protected List<string> qb_cache_select = new();
  protected List<string> qb_cache_from = new();
  protected List<string> qb_cache_join = new();

  protected List<string?> qb_no_escape = new();
  protected List<string> qb_cache_no_escape = new();

  private List<string> array(params string[] values)
  {
    return values.ToList();
  }

  public SyncBuilder select(string select = "*", bool escape = false)
  {
    var items = select.Contains(",")
      ? select.Split(",").ToList()
      : new List<string> { select };

    // (escape)OR escape = this._ProtectIdentifiers;

    items.ForEach(val =>
    {
      val = val.Trim();
      if (val == "") return;
      qb_select.Add(val);
      qb_no_escape.Add($"{escape}");
      if (qb_caching != true) return;
      qb_cache_select.Add(val);
      qb_cache_exists.Add("select");
      qb_cache_no_escape.Add($"{escape}");
    });
    return this;
  }


  public SyncBuilder select_max(string select = "", string alias = "")
  {
    return _max_min_avg_sum(select, alias);
  }

  public SyncBuilder select_min(string select = "", string alias = "")
  {
    return _max_min_avg_sum(select, alias, "MIN");
  }

  public SyncBuilder select_avg(string select = "", string alias = "")
  {
    return _max_min_avg_sum(select, alias, "AVG");
  }

  public SyncBuilder select_sum(string select = "", string alias = "")
  {
    return _max_min_avg_sum(select, alias, "SUM");
  }

  protected SyncBuilder _max_min_avg_sum(string? select = null, string alias = "", string type = "MAX")
  {
    if (string.IsNullOrEmpty(select)) this.display_error("db_invalid_query");

    type = type.ToUpper();

    if (!array("MAX", "MIN", "AVG", "SUM").Contains(type)) this.show_error($"Invalid SyncBuilder type: {type}");

    if (alias == "") alias = _create_alias_from_table(select.Trim());

    var sql = $"{type}(" + ProtectIdentifiers(select.Trim()) + ") AS ";
    qb_select.Add(sql);
    qb_no_escape.Add(null);
    if (qb_caching != true) return this;
    qb_cache_select.Add(sql);
    qb_cache_exists.Add("select");

    return this;
  }

  protected string _create_alias_from_table(string item)
  {
    var items = new List<string>();
    return item.Contains(".")
      ? item.Split(".").ToList().Last()
      : item;
  }

  public SyncBuilder distinct(bool? val = null)
  {
    qb_distinct = val ?? true;
    return this;
  }

  public SyncBuilder from(params string[] from)
  {
    foreach (var val in from)
      if (val.Contains(","))
      {
        var items = val.Split(",").ToList();
        items.ForEach(item => { });
        foreach (var v in items.Select(item => item.Trim()))
        {
          _track_aliases(v);
          var _v = ProtectIdentifiers(v, true);
          qb_from.Add(_v);
          if (qb_caching != true) return this;
          qb_cache_from.Add(_v);
          qb_cache_exists.Add("from");
        }
      }
      else
      {
        var _val = val.Trim();
        _track_aliases(val);
        qb_from.Add(_val = ProtectIdentifiers(val, true));
        if (qb_caching != true) continue;
        qb_cache_from.Add(val);
        qb_cache_exists.Add("from");
      }

    return this;
  }

  public SyncBuilder join(string table, string cond, string type = "", bool escape = false)
  {
    if (type != "")
    {
      type = type.Trim().ToUpper();

      if (!array("LEFT", "RIGHT", "OUTER", "INNER", "LEFT OUTER", "RIGHT OUTER").Contains(type))
        type = "";
      else
        type += " ";
    }

    _track_aliases(table);

    // is_bool(escape) || escape = this._ProtectIdentifiers;


    if (escape) table = ProtectIdentifiers(table, true);
    var _join = "";
    qb_join.Add(_join = type + "JOIN " + table + cond);

    if (qb_caching != true) return this;
    qb_cache_join.Add(_join);
    qb_cache_exists.Add("join");

    return this;
  }

  public SyncBuilder where(string key, object? value = null, bool escape = false)
  {
    return _wh(qb_where, key, value, "AND ", escape);
  }

  public SyncBuilder or_where(string key, object value = null, bool escape = false)
  {
    return _wh(qb_where, key, value, "OR ", escape);
  }

  protected SyncBuilder _wh(List<string> qb_key, string key, object value = null, string type = "AND ", bool escape = false)
  {
    return this;
  }

  public SyncBuilder where_in(string key = null, object? values = null, bool escape = false)
  {
    return _where_in(key, values, false, "AND ", escape);
  }

  public SyncBuilder or_where_in(string key = null, object? values = null, bool escape = false)
  {
    return _where_in(key, values, false, "OR ", escape);
  }

  public SyncBuilder where_not_in(string key = null, object? values = null, bool escape = false)
  {
    return _where_in(key, values, true, "AND ", escape);
  }

  public SyncBuilder or_where_not_in(string key = null, object values = null, bool escape = false)
  {
    return _where_in(key, values, true, "OR ", escape);
  }

  protected SyncBuilder _where_in(string key = null, object? values = null, bool not = false, string type = "AND ", bool escape = false)
  {
    return this;
  }

  public SyncBuilder like(string field, string match = "", string side = "both", bool escape = false)
  {
    return _like(field, match, "AND ", side, "", escape);
  }

  public SyncBuilder not_like(string field, string match = "", string side = "both", bool escape = false)
  {
    return _like(field, match, "AND ", side, "NOT", escape);
  }

  public SyncBuilder or_like(string field, string match = "", string side = "both", bool escape = false)
  {
    return _like(field, match, "OR ", side, "", escape);
  }

  public SyncBuilder or_not_like(string field, string match = "", string side = "both", bool escape = false)
  {
    return _like(field, match, "OR ", side, "NOT", escape);
  }

  protected SyncBuilder _like(string field, string match = "", string type = "AND ", string side = "both", string not = "", bool escape = false)
  {
    return this;
  }

  public SyncBuilder group_start(string not = "", string type = "AND ")
  {
    return this;
  }

  public SyncBuilder or_group_start()
  {
    return group_start("", "OR ");
  }

  public SyncBuilder not_group_start()
  {
    return group_start("NOT ");
  }

  public SyncBuilder or_not_group_start()
  {
    return group_start("NOT ", "OR ");
  }

  public SyncBuilder group_end()
  {
    return this;
  }

  protected string _group_get_type(string type)
  {
    if (!qb_where_group_started) return type;
    type = "";
    qb_where_group_started = false;

    return type;
  }

  public SyncBuilder group_by(string by, bool escape = false)
  {
    return this;
  }

  public SyncBuilder having(string key, object? value = null, bool escape = false)
  {
    return _wh(qb_having, key, value, "AND ", escape);
  }

  public SyncBuilder or_having(string key, object? value = null, bool escape = false)
  {
    return _wh(qb_having, key, value, "OR ", escape);
  }

  public SyncBuilder order_by(string orderby, string direction = "", bool escape = false)
  {
    return this;
  }

  public SyncBuilder limit(string value, int offset = 0)
  {
    // is_null(value) || this.qb_limit =  value;
    // string.IsNullOrEmpty(offset) || this.qb_offset =  offset;

    return this;
  }

  public SyncBuilder offset(int offset)
  {
    // string.IsNullOrEmpty(offset) || this.qb_offset =  offset;
    return this;
  }

  protected string _limit(string sql)
  {
    return string.Empty;
  }

  public SyncBuilder set(string key, object? value = null, bool escape = false)
  {
    return this;
  }

  public string get_compiled_select(string table = "", bool reset = true)
  {
    if (table != "")
    {
      _track_aliases(table);
      from(table);
    }

    var select = _compile_select();

    if (reset) _reset_select();

    return select;
  }

  // public SyncBuilder get(string table = "", int limit = null, object offset = null)
  // {
  //   if (table != "")
  //   {
  //     this._track_aliases(table);
  //     this.from(table);
  //   }

  //   if (!string.IsNullOrEmpty(limit))
  //   {
  //     this.limit(limit, offset);
  //   }

  //   result = this.query(this._compile_select());
  //   this._reset_select();
  //   return result;
  // }


  public int insert_batch(string table, string set = null, bool escape = false, int batch_size = 100)
  {
    var affected_rows = 0;
    _reset_write();
    return affected_rows;
  }

  protected string _insert_batch(string table, string keys, object values)
  {
    return "INSERT INTO " + table + " (" + string.Join(", ", keys) + ") VALUES " + string.Join(", ", values);
  }

  public SyncBuilder set_insert_batch(string key, object? value = null, bool escape = false)
  {
    return this;
  }

  public string get_compiled_insert(string table = "", bool reset = true)
  {
    return "";
  }

  public SyncBuilder insert(string table = "", string set = null, bool escape = false)
  {
    return this;
  }

  protected bool _validate_insert(string table = "")
  {
    return true;
  }

  public SyncBuilder replace(string table = "", object set = null)
  {
    return this;
  }

  protected string _replace(string table, string keys, params object[] values)
  {
    return "";
  }

  protected string _from_tables()
  {
    return string.Join(", ", qb_from);
  }

  public bool get_compiled_update(string table = "", bool reset = true)
  {
    _merge_cache();

    if (_validate_update(table) == false) return false;

    return false;
  }

  public bool update(string table = "", dynamic set = null, dynamic? where = null, int? limit = null)
  {
    return false;
  }

  protected bool _validate_update(string table)
  {
    return true;
  }

  public int update_batch(string table, string set = null, object index = null, int batch_size = 100)
  {
    return 0;
  }

  protected string _update_batch(string table, dynamic values, int index)
  {
    return string.Empty;
  }

  public string set_update_batch(string key, string index = "", bool escape = false)
  {
    return string.Empty;
  }


  public SyncBuilder empty_table(string table = "")
  {
    return this;
  }

  public SyncBuilder truncate(string table = "")
  {
    return this;
  }

  protected string _truncate(string table)
  {
    return string.Empty;
  }

  public string get_compiled_delete(string table = "", bool reset = true)
  {
    return string.Empty;
  }

  public SyncBuilder delete(string table = "", string where = "", int? limit = null, bool reset_data = true)
  {
    return this;
  }

  protected string _delete(string table)
  {
    return string.Empty;
  }

  public string dbprefix(string table = "")
  {
    if (table == "") this.display_error("db_table_name_required");

    return dbprefix + table;
  }

  public string set_dbprefix(string prefix = "")
  {
    return string.Empty;
  }

  protected SyncBuilder _track_aliases(string table)
  {
    return this;
  }

  protected string _compile_select(bool select_override = false)
  {
    var sql = "";
    _merge_cache();
    if (select_override)
    {
      sql = $"{select_override}";
    }
    else
    {
      sql = !qb_distinct ? "SELECT " : "SELECT DISTINCT ";

      if (!qb_select.Any())
        sql += "*";
      else
        // foreach (this.qb_select as key => val)
        // {
        //   no_escape = isset(this.qb_no_escape[key]) ? this.qb_no_escape[key] : null;
        //   this.qb_select[key] = this.ProtectIdentifiers(val, false, no_escape);
        // }
        sql += string.Join(", ", qb_select);
    }

    if (qb_from.Any()) sql += "\nFROM " + _from_tables();
    if (qb_join.Any()) sql += "\n" + string.Join("\n", qb_join);

    sql += _compile_wh(qb_where)
           + _compile_group_by()
           + _compile_wh(qb_having)
           + _compile_order_by(); // ORDER BY
    if (qb_limit || qb_offset) return _limit(sql + "\n");

    return sql;
  }

  protected string _compile_wh(List<string> qb_key)
  {
    return "";
  }

  protected string _compile_group_by()
  {
    return string.Empty;
  }

  protected string _compile_order_by()
  {
    return string.Empty;
  }

  protected List<string> _object_to_array(object obj)
  {
    return new List<string>();
  }

  protected List<string> _object_to_array_batch(object obj)
  {
    return default;
  }

  public SyncBuilder start_cache()
  {
    qb_caching = true;
    return this;
  }

  public SyncBuilder stop_cache()
  {
    qb_caching = false;
    return this;
  }

  public SyncBuilder flush_cache()
  {
    _reset_run(new
    {
      qb_cache_select = array(),
      qb_cache_from = array(),
      qb_cache_join = array(),
      qb_cache_where = array(),
      qb_cache_groupby = array(),
      qb_cache_having = array(),
      qb_cache_orderby = array(),
      qb_cache_set = array(),
      qb_cache_exists = array(),
      qb_cache_no_escape = array(),
      qb_cache_aliased_tables = array()
    });
    return this;
  }

  protected SyncBuilder _merge_cache()
  {
    return this;
  }

  protected bool _is_literal(string str)
  {
    return false;
  }

  public SyncBuilder reset_query()
  {
    _reset_select();
    _reset_write();
    return this;
  }

  protected void _reset_run(object qb_reset_items)
  {
    var items = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(qb_reset_items));
    items = items.ToDictionary(x => x.Key, x => x.Value);
    // items.Where(x => x.Key == "qb_select").Select(x => x.Value).ToList().ForEach(x => this.qb_select = (List<string>)x);
    foreach (var kvp in items) GetType().GetProperty(kvp.Key)?.SetValue(this, kvp.Value);
  }

  protected void _reset_select()
  {
    _reset_run(new
    {
      qb_select = array(),
      qb_from = array(),
      qb_join = array(),
      qb_where = array(),
      qb_groupby = array(),
      qb_having = array(),
      qb_orderby = array(),
      qb_aliased_tables = array(),
      qb_no_escape = array(),
      qb_distinct = false,
      qb_limit = false,
      qb_offset = false
    });
  }

  protected void _reset_write()
  {
    _reset_run(new
    {
      qb_set = array(),
      qb_set_ub = array(),
      qb_from = array(),
      qb_join = array(),
      qb_where = array(),
      qb_orderby = array(),
      qb_keys = array(),
      qb_limit = false
    });
  }
}
