using Service.Entities;

namespace Service.Models.Users;

public class StaffResultSet
{
  public Staff Staff { get; internal set; }
  public string FullName { get; internal set; }
  public int? TotalUnreadNotifications { get; internal set; }
  public int? TotalUnfinishedTodos { get; internal set; }
}
