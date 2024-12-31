using Microsoft.EntityFrameworkCore;
using Service.Entities;
using Service.Framework;
using Task = System.Threading.Tasks.Task;

namespace Service.Models.Invoices;

public class InvoiceItemsModel(MyInstance self, MyContext db) : MyModel(self)
{
  public async Task<int?> CopyAsync(Item itemData)
  {
    var newItem = new Item
    {
      Description = $"{itemData.Description} - Copy",
      Rate = itemData.Rate,
      Tax = itemData.Tax,
      Tax2 = itemData.Tax2,
      GroupId = itemData.GroupId,
      Unit = itemData.Unit,
      LongDescription = itemData.LongDescription
    };

    // Handle dynamic columns for rate_currency_ fields
    foreach (var prop in itemData.GetType().GetProperties())
      if (prop.Name.Contains("RateCurrency"))
        newItem.GetType().GetProperty(prop.Name)?.SetValue(newItem, prop.GetValue(itemData));

    await db.Items.AddAsync(newItem);
    await db.SaveChangesAsync();

    // Handle custom fields (if applicable)
    foreach (var customField in await GetCustomFieldsAsync())
    {
      var customFieldValue = await GetCustomFieldValueAsync(itemData.Id, customField.Id);
      if (customFieldValue != null)
      {
        // Logic to copy custom field values
        // e.g., newItem.CustomFields.Add(new CustomField { ... });
      }
    }

    // Add hooks if needed (e.g., logging)
    log_activity($"Copied Item [ID: {itemData.Id}, {newItem.Description}]");

    return newItem.Id;
  }

  public async Task<Item> GetAsync(int id)
  {
    return await db.Items
      .Include(i => i.Tax)
      .Include(i => i.Tax2)
      .Include(i => i.Group)
      .FirstOrDefaultAsync(i => i.Id == id);
  }

  public async Task<IEnumerable<Item>> GetGroupedAsync()
  {
    var groups = await db.ItemsGroups.OrderBy(g => g.Name).ToListAsync();
    var items = new Dictionary<int, List<Item>>();

    foreach (var group in groups)
    {
      var groupItems = await db.Items
        .Where(i => i.GroupId == group.Id)
        .OrderBy(i => i.Description)
        .ToListAsync();

      if (groupItems.Any()) items[group.Id] = groupItems;
    }

    return items.Values.SelectMany(i => i).ToList();
  }

  public async Task<int?> AddAsync(Item newItem)
  {
    // Validate and adjust the newItem before saving
    newItem.Tax ??= 0;
    newItem.Tax2 ??= 0;
    newItem.GroupId ??= 0;

    // Add the item to the database
    await db.Items.AddAsync(newItem);
    await db.SaveChangesAsync();

    log_activity($"New Invoice Item Added [ID: {newItem.Id}, {newItem.Description}]");

    return newItem.Id;
  }

  public async Task<bool> EditAsync(Item updatedItem)
  {
    var existingItem = await db.Items.FindAsync(updatedItem.Id);

    if (existingItem == null) return false;

    // Update properties
    existingItem.Description = updatedItem.Description;
    existingItem.Rate = updatedItem.Rate;
    existingItem.Tax = updatedItem.Tax ?? 0;
    existingItem.Tax2 = updatedItem.Tax2 ?? 0;
    existingItem.GroupId = updatedItem.GroupId ?? 0;
    existingItem.Unit = updatedItem.Unit;
    existingItem.LongDescription = updatedItem.LongDescription;

    db.Items.Update(existingItem);
    await db.SaveChangesAsync();

    log_activity($"Invoice Item Updated [ID: {existingItem.Id}, {existingItem.Description}]");

    return true;
  }

  public async Task<IEnumerable<Item>> SearchAsync(string query)
  {
    return await db.Items
      .Where(i => i.Description.Contains(query) || i.LongDescription.Contains(query))
      .Select(i => new Item
      {
        Id = i.Id,
        Description = i.Description,
        LongDescription = i.LongDescription.Substring(0, 200) + "...",
        Rate = i.Rate
      })
      .ToListAsync();
  }

  public async Task<bool> DeleteAsync(int id)
  {
    var item = await db.Items.FindAsync(id);
    if (item == null) return false;

    db.Items.Remove(item);
    await db.SaveChangesAsync();

    log_activity($"Invoice Item Deleted [ID: {item.Id}]");

    return true;
  }

  // Group methods
  public async Task<IEnumerable<ItemsGroup>> GetGroupsAsync()
  {
    return await db.ItemsGroups.OrderBy(g => g.Name).ToListAsync();
  }

  public async Task<int?> AddGroupAsync(ItemsGroup newGroup)
  {
    await db.ItemsGroups.AddAsync(newGroup);
    await db.SaveChangesAsync();

    log_activity($"Items Group Created [Name: {newGroup.Name}]");

    return newGroup.Id;
  }

  public async Task<bool> EditGroupAsync(ItemsGroup updatedGroup)
  {
    var existingGroup = await db.ItemsGroups.FindAsync(updatedGroup.Id);

    if (existingGroup == null) return false;

    existingGroup.Name = updatedGroup.Name;
    db.ItemsGroups.Update(existingGroup);
    await db.SaveChangesAsync();

    log_activity($"Items Group Updated [Name: {existingGroup.Name}]");

    return true;
  }

  public async Task<bool> DeleteGroupAsync(int id)
  {
    var group = await db.ItemsGroups.FindAsync(id);
    if (group == null) return false;

    // Unassign items from this group
    var items = await db.Items.Where(i => i.GroupId == id).ToListAsync();
    items.ForEach(i => i.GroupId = 0);

    db.ItemsGroups.Remove(group);
    await db.SaveChangesAsync();

    log_activity($"Item Group Deleted [Name: {group.Name}]");

    return true;
  }


  // Custom field handling (stubbed for illustration)
  private Task<IEnumerable<CustomField>> GetCustomFieldsAsync()
  {
    return Task.FromResult(new List<CustomField>().AsEnumerable());
  }

  private Task<string?> GetCustomFieldValueAsync(int itemId, int customFieldId)
  {
    return Task.FromResult<string?>(null);
  }
}
