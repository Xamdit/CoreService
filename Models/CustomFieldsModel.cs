using Service.Entities;
using Service.Framework;
using Service.Framework.Core.Extensions;
using Service.Framework.Helpers.Entities;

namespace Service.Models;

public class CustomFieldsModel(MyInstance self, MyContext db) : MyModel(self)
{
  private List<string> pdfFields = new() { "estimate", "invoice", "credit_note", "items" };
  private List<string> clientPortalFields = new() { "customers", "estimate", "invoice", "proposal", "contracts", "tasks", "projects", "contacts", "tickets", "company", "credit_note" };
  private List<string> clientEditableFields = new() { "customers", "contacts", "tasks" };

  public List<CustomField> get()
  {
    return db.CustomFields.ToList();
  }

  public CustomField get(int id)
  {
    return db.CustomFields.Where(x => x.Id == id).FirstOrDefault();
  }

  public bool add(CustomField data, bool? disable = null, bool? show_on_pdf = null)
  {
    if (disable.HasValue)
      data.Active = disable.HasValue ? disable.Value : false;
    data.ShowOnPdf = show_on_pdf.HasValue && pdfFields.Contains(data.FieldTo);
    data.ShowOnClientPortal = clientPortalFields.Contains(data.FieldTo);

    if (string.IsNullOrEmpty(data.FieldOrder.ToString())) data.FieldOrder = 0;

    // data.Slug = SlugIt($"{data.FieldTo}_{data.Name}", new { separator = "_" });
    data.Slug = self.helper.slug_it($"{data.FieldTo}_{data.Name}", "_");
    var slugsTotal = db.CustomFields.Count(x => x.Slug == data.Slug);
    if (slugsTotal > 0) data.Slug += $"_{slugsTotal + 1}";

    if (data.FieldTo is "company" or "items")
    {
      data.ShowOnPdf = true;
      data.ShowOnClientPortal = true;
      data.ShowOnTable = true;
      data.OnlyAdmin = false;
      data.DisalowClientToEdit = 0;
    }

    var result = db.CustomFields.Add(data);
    var insertId = result.Entity.Id;
    if (insertId <= 0) return false;
    log_activity($"New Custom Field Added [{data.Name}]");
    return result.IsAdded();
  }

  public bool Update(CustomField data, int id, bool? disabled = false)
  {
    var originalField = get(id);
    if (disabled.HasValue)
      data.Active = disabled.Value;
    data.ShowOnPdf = data.ShowOnPdf && pdfFields.Contains(data.FieldTo);

    if (string.IsNullOrEmpty(data.FieldOrder.ToString()))
      data.FieldOrder = 0;

    data.ShowOnClientPortal = data.ShowOnClientPortal && clientPortalFields.Contains(data.FieldTo);


    if (data.FieldTo is "company" or "items")
    {
      data.ShowOnPdf = true;
      data.ShowOnClientPortal = true;
      data.ShowOnTable = true;
      data.OnlyAdmin = false;
      data.DisalowClientToEdit = 0;
    }

    var affected_rows = db.CustomFields.Where(x => x.Id == id).Update(x => data);
    if (affected_rows <= 0) return false;

    log_activity($"Custom Field Updated [{data.Name}]");

    if (data.Type is not ("checkbox" or "select" or "multiselect")) return true;

    if (data.Options.Trim() == originalField.Options.Trim()) return true;

    var optionsNow = data.Options.ToString().Split(',').Select(val => val.Trim()).ToList();
    var optionsBefore = originalField.Options.Split(',').Select(val => val.Trim()).ToList();
    // var removedOptionsInUse = new List<string>();
    var removedOptionsInUse = optionsBefore
      .Where(
        x =>
          optionsNow.Contains(x) &&
          db.CustomFieldsValues
            .Any(y =>
              y.FieldId == id &&
              y.Value == x
            )
      )
      .ToList();
    if (!removedOptionsInUse.Any()) return true;
    db.CustomFields
      .Where(x => x.Id == id)
      .Update(x => new CustomField
      {
        Options = $"{string.Join(",", optionsNow)},{string.Join(",", removedOptionsInUse)}"
      });
    return true;
  }


  public bool Delete(int id)
  {
    var affected_rows = db.CustomFields.Where(x => x.Id == id).Delete();
    if (affected_rows <= 0) return false;
    db.CustomFieldsValues.Where(x => x.FieldId == id).Delete();
    log_activity($"Custom Field Deleted [{id}]");
    return true;
  }

  public void ChangeCustomFieldStatus(int id, bool status)
  {
    db.CustomFields.Where(x => x.Id == id)
      .Update(x => new CustomField
      {
        Active = status
      });
    log_activity($"Custom Field Status Changed [FieldId: {id} - Active: {status}]");
  }

  public List<string> GetPdfAllowedFields()
  {
    return pdfFields;
  }

  public List<string> GetClientPortalAllowedFields()
  {
    return clientPortalFields;
  }

  public List<string> GetClientEditableFields()
  {
    return clientEditableFields;
  }
}
