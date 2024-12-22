namespace Service.Components.Layout.Partials.SidebarLayout.Sidebar;

public class SidebarMenuItem
{
  public string Title { get; set; }
  public string Icon { get; set; }
  public string Url { get; set; }
  public bool IsActive { get; set; }
  public List<SidebarMenuItem>? Submenu { get; set; } = null;
}
