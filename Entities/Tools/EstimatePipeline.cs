using Microsoft.EntityFrameworkCore;
using Service.Framework;
using Service.Helpers;

namespace Service.Entities.Tools;

public class EstimatesPipeline(MyInstance instance, int status) : AbstractKanban(instance, status)
{
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
    var db = self.db();
    var has_permission_view = self.helper.has_permission("estimates", "", "view");
    var noPermissionQuery = self.helper.get_estimates_where_sql_for_staff(self.helper.get_staff_user_id());
    var query = db.Estimates
      .Include(e => e.Client)
      .Include(e => e.Currency)
      .Where(e => e.Status == status)
      .AsQueryable();
    if (!has_permission_view)
      query = query.Where(noPermissionQuery);
    return this;
  }

  protected override void applySearchQuery(string q)
  {
    throw new NotImplementedException();
  }

  protected override string defaultSortDirection()
  {
    var (self, db) = getInstance();
    return db.get_option("default_estimates_pipeline_sort_type");
  }

  protected override string defaultSortColumn()
  {
    var (self, db) = getInstance();
    return db.get_option("default_estimates_pipeline_sort");
  }

  protected override int limit()
  {
    return 0;
  }
}