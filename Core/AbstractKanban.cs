using Global.Entities;
using Service.Framework;

namespace Service.Core;

public abstract class AbstractKanban
{
  protected int my_limit { get; set; }
  protected string default_sort;
  protected string default_sort_direction;
  protected int status;
  protected int page_count = 1;
  protected int? refreshAtTotal;
  protected string q;
  protected Action queryTapCallback;
  private string sort_by_column;
  private string sort_direction;
  public MyInstance self { get; set; }
  public List<Estimate> rows = new();

  public AbstractKanban(MyInstance instance, int status)
  {
    self = instance;
    this.status = status;
    my_limit = limit();
    sort_by_column = defaultSortColumn();
    sort_direction = defaultSortDirection();
  }


  public AbstractKanban tapQuery(Action callback)
  {
    queryTapCallback = callback;
    return this;
  }

  public int totalPages()
  {
    return (int)Math.Ceiling(countAll() / my_limit);
  }

  public List<Estimate> get()
  {
    var output = rows;
    if (refreshAtTotal is > 0)
    {
      page_count = (int)Math.Ceiling((double)refreshAtTotal.Value / my_limit);
      var allPagesTotal = page_count * my_limit;
      output = output.Take(allPagesTotal > refreshAtTotal ? refreshAtTotal.Value : allPagesTotal).ToList();
    }
    else
    {
      if (page_count > 1)
      {
        var position = (page_count - 1) * my_limit;
        output = output.Skip(position).Take(my_limit).ToList();
      }
      else
      {
        output = output.Take(my_limit).ToList();
      }
    }

    initiateQuery();

    if (!string.IsNullOrEmpty(q)) applySearchQuery(q);

    applySortQuery();
    tapQueryIfNeeded();
    return output;
  }

  public decimal countAll()
  {
    initiateQuery();
    if (!string.IsNullOrEmpty(q)) applySearchQuery(q);
    tapQueryIfNeeded();
    return rows.Count();
  }

  public AbstractKanban refresh(int atTotal)
  {
    refreshAtTotal = atTotal;
    return this;
  }

  public AbstractKanban page(int page)
  {
    page_count = page;
    return this;
  }

  public int getPage()
  {
    return page_count;
  }

  public AbstractKanban sortBy(string column, string direction)
  {
    if (string.IsNullOrEmpty(column) || string.IsNullOrEmpty(direction)) return this;
    sort_by_column = column;
    sort_direction = direction;

    return this;
  }

  public AbstractKanban search(string q)
  {
    this.q = q;
    return this;
  }

  protected void applySortQuery()
  {
    if (string.IsNullOrEmpty(sort_by_column) || string.IsNullOrEmpty(sort_direction)) return;
    var nullsLast = $"{qualifyColumn(sort_by_column)} IS NULL {sort_direction}";
    var actualSort = $"{qualifyColumn(sort_by_column)} {sort_direction}";
    // ci.db.order_by($"{nullsLast}, {actualSort}", "", false);
  }

  protected void tapQueryIfNeeded()
  {
    queryTapCallback?.Invoke();
  }

  protected string qualifyColumn(string column)
  {
    return $"{table()}.{column}";
  }

  public static void updateOrder(List<Tuple<int, int>> data, string column, string table, string status, string statusColumnName = "status", string primaryKey = "id", dynamic ciInstance = null)
  {
    var batch = new List<Dictionary<string, object>>();
    var allIds = new List<int>();
    var allOrders = new List<int>();

    foreach (var order in data)
    {
      allIds.Add(order.Item1);
      allOrders.Add(order.Item2);
      batch.Add(new Dictionary<string, object>
      {
        { primaryKey, order.Item1 },
        { column, order.Item2 }
      });
    }

    var maxOrder = allOrders.Max();
    var updateQuery = $"UPDATE {table} SET {column} = {maxOrder} + {column} WHERE {primaryKey} NOT IN ({string.Join(",", allIds)}) AND {statusColumnName} = '{status}'";

    ciInstance.db.query(updateQuery);
    ciInstance.db.update_batch(table, batch, primaryKey);
  }

  protected abstract string table();

  protected abstract AbstractKanban initiateQuery();

  protected abstract void applySearchQuery(string q);

  protected abstract string defaultSortDirection();

  protected abstract string defaultSortColumn();

  protected abstract int limit();
}
