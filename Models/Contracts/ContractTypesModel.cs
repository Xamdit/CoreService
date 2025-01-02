using Service.Entities;
using Service.Framework;
using Service.Framework.Helpers.Entities;
using Service.Helpers;

namespace Service.Models.Contracts;

public class ContractTypesModel(MyInstance self, MyContext db) : MyModel(self, db)
{
  /**
      * Add new contract type
      * @param mixed data All _POST data
      */
  public int add(ContractsType data)
  {
    var result = db.ContractsTypes.Add(data);
    if (result.Entity.Id <= 0) return 0;
    log_activity($"New Contract Type Added [{data.Name}]");
    return result.Entity.Id;
  }

  /**
   * Edit contract type
   * @param mixed data All _POST data
   * @param mixed id Contract type id
   */
  public bool update(ContractsType data)
  {
    var id = data.Id;
    var result = db.ContractsTypes.Where(x => x.Id == id).Update(x => data);
    if (result > 0) return false;
    log_activity($"Contract Type Updated [{data.Name}, ID:{id}]");
    return true;
  }

  /**
   * @param  integer ID (optional)
   * @return mixed
   * Get contract type object based on passed id if not passed id return array of all types
   */
  public List<ContractsType> get()
  {
    var types = app_object_cache.get<List<ContractsType>>("contract-types");
    if (types == null) return types;
    types = db.ContractsTypes.ToList();
    app_object_cache.add("contract-types", types);
    return types;
  }

  public ContractsType get(int id)
  {
    var output = db.ContractsTypes.Find(id);
    return output;
  }

  /**
   * @param  integer ID
   * @return mixed
   * Delete contract type from database, if used return array with key referenced
   */
  public bool delete(int id)
  {
    // if (db.is_reference_in_table<ContractsType>(  "contracts", id)) {
    //     return new{                referenced = true,            };
    // }
    var result = db.ContractsTypes.Where(x => x.Id == id).Delete();
    if (result <= 0) return false;
    log_activity($"Contract Deleted [{id}]");
    return true;
  }

  /**
   * Get contract types data for chart
   * @return array
   */
  public async Task<Chart> get_chart_data()
  {
    var labels = new List<string>();
    var totals = new List<int>();
    var types = get();
    foreach (var type in types)
    {
      var total_rows_where = CreateCondition<Contract>(x => x.ContractType == type.Id > 0 && x.Trash == false);
      if (db.is_client_logged_in())
      {
        total_rows_where = total_rows_where.And(x =>
          x.Client == db.get_client_user_id()
          && string.IsNullOrEmpty(x.NotVisibleToClient)
        );
      }
      else
      {
        var view_contract = db.has_permission("contracts", 0, "view");
        if (!view_contract) total_rows_where = total_rows_where.And(x => x.AddedFrom == db.get_staff_user_id());
      }

      var _total_rows = db.Contracts.Count(total_rows_where);
      if (_total_rows == 0 && db.is_client_logged_in()) continue;
      labels.Add(type.Name);
      totals.Add(_total_rows);
    }

    var chart = new Chart
    {
      Labels = labels,
      Datasets = new List<ChartDataset>
      {
        new()
        {
          Label = label("contract_summary_by_type"),
          BackgroundColor = "rgba(3,169,244,0.2)",
          BorderColor = "#03a9f4",
          BorderWidth = 1,
          Data = totals
        }
      }
    };

    return chart;
  }

  /**
   * Get contract types values for chart
   * @return array
   */
  public async Task<Chart> get_values_chart_data()
  {
    var labels = new List<string>();
    var totals = new List<int>();
    var types = get();
    foreach (var type in types)
    {
      labels.Add(type.Name);
      var query = db.Contracts.Where(x => x.ContractType == (type.Id == 1) && x.Trash == false).AsQueryable();
      var view_contract = db.has_permission("contracts", 0, "view");
      if (!view_contract)
      {
        var staff_user_id = db.get_staff_user_id();
        query = query.Where(x => x.AddedFrom == staff_user_id);
      }

      var total = query.Select(x => x.ContractValue).Sum();
      // if (total == null) total = 0;
      totals.Add(total);
    }

    var chart = new Chart
    {
      Labels = labels,
      Datasets = new List<ChartDataset>
      {
        new()
        {
          Label = label("contract_summary_by_type_value"),
          BackgroundColor = "rgba(37,155,35,0.2)",
          BorderColor = "#84c529",
          Tension = false,
          BorderWidth = 1,
          Data = totals
        }
      }
    };

    return chart;
  }
}
