using Service.Framework;

namespace Service.Libraries;

public class AppMenuItem
{
  public string Slug { get; set; }
  public string Name { get; set; }
  public string Group { get; set; }
  public string Icon { get; set; }
  public string Url { get; set; }
  public string Position { get; set; }
  public string ParentSlug { get; set; }
  public List<AppMenuItem> Children { get; set; }
}

public class AppMenu(MyInstance self)
{
  private List<AppMenuItem> items = new();
  private Dictionary<string, Dictionary<string, List<AppMenuItem>>> child = new();
  private AppMenuItem userMenuItems = new();


  public AppMenu AddSidebarMenuItem(string slug, AppMenuItem item)
  {
    Add(slug, item, "sidebar");
    return this;
  }

  public AppMenu AddSidebarChildrenItem(string parentSlug, AppMenuItem item)
  {
    AddChild(parentSlug, item, "sidebar");
    return this;
  }

  public List<AppMenuItem> GetSidebarMenuChildItems(string parentSlug)
  {
    return get_child(parentSlug, "sidebar");
  }

  public AppMenuItem GetSidebarMenuItems()
  {
    return get("sidebar");
  }

  public AppMenu AddSetupMenuItem(string slug, AppMenuItem item)
  {
    Add(slug, item, "setup");
    return this;
  }

  public AppMenu AddSetupChildrenItem(string parentSlug, AppMenuItem item)
  {
    AddChild(parentSlug, item, "setup");
    return this;
  }

  public List<AppMenuItem> GetSetupMenuChildItems(string parentSlug)
  {
    return get_child(parentSlug, "setup");
  }

  public AppMenuItem GetSetupMenuItems()
  {
    return get("setup");
  }

  public AppMenu AddThemeItem(string slug, AppMenuItem item)
  {
    Add(slug, item, "theme");
    return this;
  }

  public AppMenuItem GetThemeItems()
  {
    return get("theme");
  }

  public AppMenu AddUserMenuItem(string slug, AppMenuItem item)
  {
    item = AppFillEmptyCommonAttributes(item);
    item.Slug = slug;
    // userMenuItems[slug] = item;
    userMenuItems = item;
    return this;
  }

  public List<AppMenuItem> GetUserMenuItems()
  {
    var items = ApplyFilters("nav_user_menu_items", userMenuItems);
    return AppSortByPosition(items);
  }

  private void Add(string slug, AppMenuItem item, string group)
  {
    item = AppFillEmptyCommonAttributes(item);
    item.Slug = slug;
    if (!items.Any(x => x.Group == group))
      items.Add(new AppMenuItem
      {
        Group = group,
        Slug = slug
      });

    items
      .Where(x => x.Group == group)
      .ToList()
      .ForEach(x => { x.Slug = slug; });
  }

  private void AddChild(string parentSlug, AppMenuItem item, string group)
  {
    item = AppFillEmptyCommonAttributes(item);
    item.ParentSlug = parentSlug;

    if (!child.ContainsKey(group)) child[group] = new Dictionary<string, List<AppMenuItem>>();

    if (!child[group].ContainsKey(parentSlug)) child[group][parentSlug] = new List<AppMenuItem>();

    child[group][parentSlug].Add(item);
  }

  private AppMenuItem get(string group)
  {
    var itemsGroup = items.Any(x => x.Group == group)
      ? items.First(x => x.Group == group)
      : new AppMenuItem();

    // itemsGroup.Children = get_child(parent, group);
    // foreach (var parent in itemsGroup.Keys.ToList())
    //   itemsGroup[parent]["children"] = get_child(parent, group);
    hooks.apply_filters($"{group}_menu_items", itemsGroup);
    return itemsGroup;
  }

  private List<AppMenuItem> get_child(string parentSlug, string group)
  {
    var children = child.ContainsKey(group) && child[group].ContainsKey(parentSlug) ? child[group][parentSlug] : new List<AppMenuItem>();
    var output = hooks.apply_filters($"{group}_menu_child_items", children);
    return output;
  }

  private AppMenuItem FilterItem(IEnumerable<AppMenuItem> items, string slug)
  {
    foreach (var item in items)
    {
      if (item.Slug == slug) return item;

      if (!item.Children.Any()) continue;
      foreach (var child in item.Children.Where(child => child.Slug == slug))
        return child;
    }

    return null;
  }

  public string get_initial_icon(string slug, string group)
  {
    var itemsGroup = items.Where(x => x.Name == group).ToList();
    itemsGroup.Select(parent =>
    {
      parent.Children = get_child(parent.Slug, group);
      return parent;
    });


    var temp = itemsGroup
      .Select(x =>
      {
        if (x.Slug == slug)
          return string.IsNullOrEmpty(x.Icon)
            ? x.Icon
            : string.Empty;
        x.Children = get_child(x.Slug, group);
        var items = x.Children
          .Where(child => child.Slug == slug)
          .Select(child =>
            string.IsNullOrEmpty(child.Icon)
              ? child.Icon
              : string.Empty
          )
          .ToList();
        return items.First();
      })
      .ToList()
      .FirstOrDefault();
    return temp;
  }

  // Placeholder methods for missing functionality
  private AppMenuItem AppFillEmptyCommonAttributes(AppMenuItem item)
  {
    // Implement your logic to fill common attributes
    return item;
  }

  private AppMenuItem ApplyFilters(string filterName, object items)
  {
    // Implement your logic for filtering items
    return (AppMenuItem)items; // Placeholder
  }

  private List<AppMenuItem> AppSortByPosition(object items)
  {
    // Implement your logic for sorting items
    return (List<AppMenuItem>)items; // Placeholder
  }
}
