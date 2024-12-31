using System.Linq.Expressions;
using Service.Entities;
using Service.Framework;
using Service.Helpers;

namespace Service.Models;

public class AnnouncementsModel(MyInstance self, MyContext db) : MyModel(self)
{
  /**
     * Get announcements
     * @param  string id    optional id
     * @param  array  where perform where
     * @param  string limit
     * @return mixed
     */
  public async Task<List<Announcement>?> Find(int id)
  {
    return await Find(id, x => true, 0);
  }

  public async Task<List<Announcement>?> Find(int id, Expression<Func<Announcement, bool>> where)
  {
    return await Find(id, where, 0);
  }

  public async Task<List<Announcement>?> Find(int id, Expression<Func<Announcement, bool>> where, int limit)
  {
    var query = db.Announcements.Where(where).AsQueryable();
    if (id > 0) return query.Where(x => x.Id == id).ToList();
    if (limit > 0)
    {
      var announcements = app_object_cache.get<List<Announcement>>("all-user-announcements");
      if (announcements != null) return announcements;
      query = await _annoucements_query(query);
      announcements = query.ToList();
      app_object_cache.add("all-user-announcements", announcements);
      return announcements;
    }

    query = await _annoucements_query(query);
    if (limit > 0) query.Take(limit);
    return query.ToList();
  }

  /**
     * Get total dismissed announcements for logged in user
     * @return mixed
     */
  public async Task<int> get_total_undismissed_announcements()
  {
    if (!self.helper.is_logged_in())
      return 0;
    var isClientLoggedIn = self.helper.is_client_logged_in();
    var staff = !isClientLoggedIn;
    var userid = isClientLoggedIn ? self.helper.get_contact_user_id() : self.helper.get_staff_user_id();
    var items = db.Announcements.Where(x => db.DismissedAnnouncements.Where(y => y.IsStaff == staff && y.UserId == userid).Select(y => y.AnnouncementId).ToList().Contains(x.Id)).ToList();
    items = staff
      ? items.Where(x => x.ShowToStaff).ToList()
      : items.Where(x => x.ShowToUsers).ToList();
    return items.Count();
  }

  /**
   * @param _POST array
   * @return Insert ID
   * Add new announcement calling this function
   */
  public int add(Announcement data)
  {
    data.DateCreated = DateTime.Now;
    var staff = db.Staff.Find(staff_user_id);
    data.Staff = staff;
    data.StaffId = staff.Id;
    // data = self.hooks.apply_filters("before_announcement_added", data);
    db.Announcements.Add(data);
    db.SaveChanges();
    var insertId = data.Id;
    // self.hooks.do_action("announcement_created", insert_id);
    // log_activity($"New Announcement Added [{data.Name}]");
    return insertId;
  }

  /**
   * @param  _POST array
   * @param  integer
   * @return boolean
   * This function updates announcement
   */
  public bool update(Announcement data)
  {
    data = self.hooks.apply_filters("before_announcement_updated", data);
    var result = db.Announcements.Where(x => x.Id == data.Id).Update(x => data);
    db.SaveChanges();
    if (result <= 0) return false;
    self.hooks.do_action("announcement_updated", data.Id);
    log_activity($"Announcement Updated [{data.Name}]");
    return true;
  }

  /**
   * @param  integer
   * @return boolean
   * Delete Announcement
   * All Dimissed announcements from database will be cleaned
   */
  public bool delete(int id)
  {
    // self.hooks.do_action("before_delete_announcement", id);
    db.Announcements.Where(x => x.Id == id).Delete();
    var result = db.SaveChanges();
    if (result <= 0) return false;
    db.DismissedAnnouncements.Where(x => x.AnnouncementId == id).Delete();
    db.SaveChanges();
    self.hooks.do_action("announcement_deleted", id);
    log_activity($"Announcement Deleted [{id}]");
    return true;
  }

  public void set_announcements_as_read_except_last_one(int user_id, bool staff = false)
  {
    var query = db.Announcements.AsQueryable();
    query = !staff
      ? query.Where(x => x.ShowToUsers)
      : query.Where(x => x.ShowToStaff);

    query = query.Where(x => x.Id == db.Announcements.Max(x => x.Id));
    var lastAnnouncement = query.FirstOrDefault();
    if (lastAnnouncement == null) return;
    // Get all announcements and set it to read.
    query = db.Announcements.AsQueryable();
    query = !staff
      ? query.Where(x => x.ShowToUsers)
      : query.Where(x => x.ShowToStaff);
    var announcements = query.ToList();
    announcements.ForEach(announcement =>
    {
      db.DismissedAnnouncements.Add(new DismissedAnnouncement
      {
        AnnouncementId = announcement.Id,
        IsStaff = staff,
        UserId = user_id,
        DateRead = DateTime.Now
      });
    });
  }

  private async Task<IQueryable<Announcement>> _annoucements_query(IQueryable<Announcement> query)
  {
    if (self.helper.is_client_logged_in()) query.Where(x => x.ShowToUsers);
    else if (self.helper.is_staff_logged_in()) query.Where(x => x.ShowToStaff);
    return query.OrderByDescending(x => x.DateCreated);
  }
}
