using Service.Entities;
using Service.Framework;
using Service.Framework.Core.Extensions;

namespace Service.Models;

public class TodoModel(MyInstance self, MyContext db) : MyModel(self, db)
{
  public int todo_limit = hooks.apply_filters("todos_limit", 10);

  public void set_todos_limit(int limit)
  {
    todo_limit = limit;
  }

  public int get_todos_limit()
  {
    return todo_limit;
  }


  public List<Todo> get()
  {
    var id = db.get_staff_user_id();
    return db.Todos.Where(x => x.StaffId == id).ToList();
  }

  public Todo? get(int id)
  {
    return db.Todos.FirstOrDefault(x => x.StaffId == id && x.Id == id);
  }

  /**
   * Get all user todos
   * @param  boolean finished is finished todos or not
   * @param  mixed page     pagination limit page
   * @return array
   */
  public async Task<List<Todo>> get_todo_items(int finished, int page = 0)
  {
    var staffId = db.get_staff_user_id();
    var query = db.Todos
      .Where(x => x.Finished == finished && x.StaffId == staffId)
      .OrderBy(x => x.ItemOrder)
      .AsQueryable();
    // this.db.select();
    // this.db.from('todos');
    // this.db.where('finished', finished);
    // this.db.where('staffid', get_staff_user_id());
    // this.db.order_by('item_order', 'asc');
    var position = page * todo_limit;
    query = page != 0
      ? query.Take(todo_limit).Skip(position)
      : query.Take(todo_limit);
    var todos = query.ToList();
    // format date
    return todos.Select(todo =>
    {
      todo.DateCreated = todo.DateCreated;
      todo.DateFinished = todo.DateFinished;
      todo.Description = self.helper.check_for_links(todo.Description);
      return todo;
    }).ToList();
  }

  /**
   * Add new user todo
   * @param mixed data todo _POST data
   */
  public async Task<int> add(Todo data)
  {
    data.DateCreated = DateTime.Now;
    data.Description = data.Description.nl2br();
    var staffId = db.get_staff_user_id();
    if (staffId != null) data.StaffId = staffId;
    var result = await db.Todos.AddAsync(data);
    await db.SaveChangesAsync();
    return data.Id;
  }

  public bool update(int id, Todo data)
  {
    data.Description = data.Description.nl2br();
    db.Todos.Update(data);
    var affectedRows = db.SaveChanges();
    return affectedRows > 0;
  }

  /**
   * Update todo's order / Ajax - Sortable
   * @param  mixed data todo _POST data
   */
  public void update_todo_items_order(params Todo[] data)
  {
    var dataset = data.ToList();
    dataset = dataset.Select((x, i) =>
    {
      x.ItemOrder = i;
      x.DateFinished = x.Finished == 1 ? today() : null;
      return x;
    }).ToList();
    db.Todos.UpdateRange(dataset);
    db.SaveChanges();
  }

  /**
   * Delete todo
   * @param  mixed id todo id
   * @return boolean
   */
  public async Task<bool> delete_todo_item(int id)
  {
    var staffId = db.get_staff_user_id();
    db.RemoveRange(db.Todos.Where(x => x.Id == id && x.StaffId == staffId));
    var affectedRows = await db.SaveChangesAsync();
    return affectedRows > 0;
  }

  /**
   * Change todo status / finished or not finished
   * @param  mixed id     todo id
   * @param  integer status can be passed 1 or 0
   * @return array
   */
  public async Task<dynamic> change_todo_status(int id, int status)
  {
    var staffId = db.get_staff_user_id();
    await db.Todos.Where(x => x.Id == id && x.StaffId == staffId)
      .UpdateAsync(x => new Todo { Finished = status, DateFinished = today() });
    var affectedRows = await db.SaveChangesAsync();
    return affectedRows > 0
      ? new { success = true }
      : new { success = false };
  }
}
