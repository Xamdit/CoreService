using Microsoft.EntityFrameworkCore;
using Service.Entities;
using Service.Helpers;

namespace Service.Framework.Helpers.Entities.Tools;

public class EstimatesPipeline(MyInstance instance, int status) : AbstractKanban(instance, status)
{
  public MyContext db = new();
  //
  //
  // public void limit()
  // {
  //   return db.get_option('estimates_pipeline_limit');
  // }
  //
  // protected void applySearchQuery($q)
  // {
  //   if (!startsWith($q, '#')) {
  //     $fields_client =  this->ci->db->list_fields(  'clients');
  //       $fields_estimates =  this->ci->db->list_fields(  'estimates');
  //       $q =  this->ci->db->escape_like_str($q);
  //
  //     $where = '(';
  //       $i = 0;
  //     foreach ($fields_client as $f) {
  //       $where.=   'clients.'. $f. ' LIKE "%'. $q. '%" ESCAPE \'!\'';
  //         $where.= ' OR ';
  //         $i++;
  //     }
  //     $i = 0;
  //     foreach ($fields_estimates as $f) {
  //       $where.=   'estimates.'. $f. ' LIKE "%'. $q. '%" ESCAPE \'!\'';
  //         $where.= ' OR ';
  //         $i++;
  //     }
  //     $where = substr($where, 0, -4);
  //     $where.= ')';
  //        this->ci->db->where($where);
  //   } else {
  //      this->ci->db->where(  'estimates.id IN
  //     (SELECT rel_id FROM ' . db_prefix() . 'taggables WHERE tag_id IN
  //       (SELECT id FROM ' . db_prefix() . 'tags WHERE name = "' .  this->ci->db->escape_str(strafter($search, '#')) . '")
  //     AND ' . db_prefix() . 'taggables.rel_type = \'estimate\' GROUP BY rel_id HAVING COUNT(tag_id) = 1)
  //     ');
  //   }
  //
  //   return  this;
  // }
  //
  // protected void initiateQuery()
  // {
  //   var has_permission_view = has_permission('estimates', '', 'view');
  //     var noPermissionQuery = get_estimates_where_sql_for_staff(get_staff_user_id());
  //      this->ci->db->select(  'estimates.id,status,invoiceid,'.get_sql_select_client_company(). ',total,currency,symbol,'.  'currencies.name as currency_name,date,expirydate,clientid');
  //      this->ci->db->from('estimates');
  //      this->ci->db->join(  'clients',   'clients.userid = '.  'estimates.clientid', 'left');
  //      this->ci->db->join(  'currencies',   'estimates.currency = '.  'currencies.id');
  //      this->ci->db->where('status',   this->status);
  //
  //   if (!has_permission_view) {
  //      this->ci->db->where($noPermissionQuery);
  //   }
  //
  //   return  this;
  // }


  protected override string table()
  {
    return "estimates";
  }

  protected override AbstractKanban initiateQuery()
  {
    return this;
  }

  protected override void applySearchQuery(string q)
  {
  }

  protected override string defaultSortDirection()
  {
    return string.Empty;
  }

  protected override string defaultSortColumn()
  {
    return string.Empty;
  }

  protected AbstractKanban initiate_query()
  {
    var has_permission_view = db.has_permission("estimates", "", "view");
    var noPermissionQuery = db.get_estimates_where_sql_for_staff(db.get_staff_user_id());
    var query = db.Estimates
      .Include(e => e.Client)
      .Include(e => e.Currency)
      .Where(e => e.Status == status)
      .AsQueryable();
    if (!has_permission_view)
      query = query.Where(noPermissionQuery);
    return this;
  }

  protected void apply_search_query(string q)
  {
  }

  protected string default_sort_direction()
  {
    return db.get_option("default_estimates_pipeline_sort_type");
  }

  protected string default_sort_column()
  {
    return db.get_option("default_estimates_pipeline_sort");
  }

  protected override int limit()
  {
    return 0;
  }
}
