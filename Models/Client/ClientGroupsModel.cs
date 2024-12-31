using Service.Entities;
using Service.Framework;
using Service.Framework.Schemas;

namespace Service.Models.Client;

public class ClientGroupsModel(MyInstance self, MyContext db) : MyModel(self)
{
  public async Task<DeletedResult> Delete(int id)
  {
    var db = new MyContext();
    var output = new DeletedResult();
    db.CustomersGroups.Where(x => x.Id == id).ToList().ForEach(x => db.CustomersGroups.Remove(x));
    var affectedRows = await db.SaveChangesAsync();
    if (affectedRows <= 0) return DeletedResult.Compile();
    db.CustomerGroups.Where(x => x.GroupId == id).ToList().ForEach(x => db.CustomerGroups.Remove(x));
    await db.SaveChangesAsync();
    self.hooks.do_action("customer_group_deleted", id);
    log_activity($"Customer Group Deleted [ID:{id}]");
    output.Success = true;
    return output;
  }


  /**
  * Add new customer group
  * @param array $data $_POST data
  */
  public bool add(CustomersGroup data)
  {
    var db = new MyContext();
    db.CustomersGroups.Add(data);
    db.SaveChanges();
    var insertId = data.Id;
    if (insertId <= 0) return false;
    log_activity($"New Customer Group Created [ID:{insertId}, Name:{data.Name}]");
    return true;
  }

  /**
  * Get customer groups where customer belongs
  * @param  mixed $id customer id
  * @return array
  */
  public List<CustomerGroup> get_customer_groups(int id)
  {
    var db = new MyContext();
    return db.CustomerGroups.Where(x => x.CustomerId == id).ToList();
  }

  /**
   * Get all customer groups
   * @param  string $id
   * @return mixed
   */
  public List<CustomersGroup> get_groups()
  {
    var db = new MyContext();
    return db.CustomersGroups.OrderBy(x => x.Name).ToList();
  }

  public CustomersGroup? get_groups(int id)
  {
    var db = new MyContext();
    return db.CustomersGroups.FirstOrDefault(x => x.Id == id);
  }

  /**
   * Edit customer group
   * @param  array $data $_POST data
   * @return boolean
   */
  public bool edit(CustomersGroup data)
  {
    var db = new MyContext();
    var group = db.CustomersGroups.FirstOrDefault(x => x.Id == data.Id);
    if (group == null) return false;
    group.Name = data.Name;
    var affected_rows = db.SaveChanges();
    if (affected_rows <= 0) return false;
    log_activity($"Customer Group Updated [ID : {data.Id}]");
    return true;
  }


  /**
  * Update/sync customer groups where belongs
  * @param  mixed $id        customer id
  * @param  mixed  groups_in
  * @return boolean
  */
  public bool sync_customer_groups(int id, params int[] groups_inner)
  {
    var db = new MyContext();
    var groups_in = groups_inner.ToList();
    var affectedRows = 0;
    var customer_groups = get_customer_groups(id);
    if (customer_groups.Any())
    {
      customer_groups.ForEach(customer_group =>
      {
        if (groups_in.Any())
        {
          if (groups_in.Contains(customer_group.GroupId)) return;
          db.CustomerGroups.Where(x => x.CustomerId == id && x.Id == customer_group.Id).ToList()
            .ForEach(x => { db.Remove((object)x); });
          var affected_rows = db.SaveChanges();
          if (affected_rows > 0) affectedRows++;
        }
        else
        {
          db.CustomerGroups.Where(x => x.CustomerId == id).ToList().ForEach(x => db.Remove(x));
          var affected_rows = db.SaveChanges();
          if (affected_rows > 0) affectedRows++;
        }
      });
    }
    else
    {
      if (groups_in.Any())
        groups_in.ForEach(group =>
        {
          // if (group) continue;
          db.CustomerGroups.Add(new CustomerGroup
          {
            CustomerId = id,
            GroupId = group
          });
          var affected_rows = db.SaveChanges();
          if (affected_rows > 0) affectedRows++;
        });
    }

    return affectedRows > 0;
  }
}
