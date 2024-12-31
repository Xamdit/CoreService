using System.Linq.Expressions;
using Service.Entities;
using Service.Framework;

namespace Service.Models;

public class TemplatesModel(MyInstance self, MyContext db) : MyModel(self)
{
  public int create(Template data)
  {
    data = self.hooks.apply_filters("before_template_added", data);
    db.Templates.Add(data);
    db.SaveChanges();
    if (data.Id <= 0) return 0;
    log_activity($"New Template Added [ID: {data.Id}, {data.Name}]");
    self.hooks.do_action("new_template_added", data.Id);
    return data.Id;
  }

  /**
   * Get templates by string
   *
   * @param string type
   * @param array where
   *
   * @return array
   */
  public List<Template> get_by_type(string type, Expression<Func<Template, bool>> where)
  {
    var rows = db.Templates.Where(where).Where(x => x.Type == type).OrderBy(x => x.Name).ToList();
    return rows;
  }

  /**
   * Find template by given id
   *
   * @return \stdClass
   */
  public Template? find(int id)
  {
    var row = db.Templates.FirstOrDefault(x => x.Id == id);
    return row;
  }

  /**
   * Update template
   *
   * @param  int id
   * @param  array data
   *
   * @return boolean
   */
  public bool update(Template data)
  {
    var id = data.Id;
    data = self.hooks.apply_filters("before_template_updated", data);
    var name = find(id).Name;
    db.Templates.Where(x => x.Id == id).Update(x => data);
    if (db.SaveChanges() <= 0) return false;
    log_activity($"Template updated [Name: {name}]");
    self.hooks.do_action("after_template_updated", id);
    return true;
  }

  /**
   * Delete template
   * @param  array id
   *
   * @return boolean
   */
  public bool delete(int id)
  {
    self.hooks.do_action("before_template_deleted", id);
    var name = find(id)?.Name;
    db.Templates.Where(x => x.Id == id).Delete();
    log_activity($"Template Deleted [Name: {name}]");
    self.hooks.do_action("after_template_deleted", id);
    return true;
  }
}
