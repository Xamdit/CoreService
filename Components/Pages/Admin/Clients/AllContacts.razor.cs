// <copyright file="AllContacts.razor.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Components;

namespace Service.Components.Pages.Admin.Clients;

public class AllContactsRazor : XComponentBase
{
  [Parameter] public string Name { get; set; }
  public List<dynamic> ConsentPurposes = new();

  /// <inheritdoc/>
  protected override void OnInitialized()
  {
    // LocalStorage.SetItem("name", "John Smith");
    // var name = LocalStorage.GetItem<string>("name");
    // Console.WriteLine("admin area check");
    var tableData = new List<string>
    {
      text("client_firstname"),
      text("client_lastname")
    };
    // if (is_gdpr() && real("gdpr_enable_consent_for_contacts"))
    var temp = new
    {
      name = text("gdpr_consent") + " (" + text("gdpr_short") + " )",
      th_attrs = new
      {
        id = "th-consent"
        // "class" = "not-export"
      }
    };
    // table_data = TypeMerger.Merge(table_data, new List<string>
    // {
    //   @text("client_email"),
    //   @text("clients_list_company"),
    //   @text("client_phonenumber"),
    //   @text("contact_position"),
    //   @text("clients_list_last_login"),
    //   @text("contact_active")
    // });
    // var custom_fields = get_custom_fields("contacts", new { show_on_table = 1 });
    // foreach (var field in custom_fields)
    // {
    //   array_push(table_data, field['name']);
    // }
    // render_datatable(table_data, "all-contacts");
  }
}
