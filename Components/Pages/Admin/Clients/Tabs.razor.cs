// <copyright file="Tabs.razor.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Dynamic;
using Microsoft.AspNetCore.Components;
using Service.Core.Engine;
using Service.Core.Extensions;
using Service.Schemas.Ui.Entities;
using Task = System.Threading.Tasks.Task;

namespace Service.Components.Pages.Admin.Clients;

public class TabsRazor : MyComponentBase
{
  public string ClassName = string.Empty;
  public Dictionary<string, Tab> CustomerTabs = new();

  [Parameter] public dynamic Client { get; set; } = new ExpandoObject();

  public string GetClassName(string key)
  {
    return key == "profile" ? "active" : $"customer_tab_{key}";
  }

  public Dictionary<string, Tab> filter_client_visible_tabs()
  {
    var tabs = new Dictionary<string, Tab>();
    return tabs;
  }

  /// <inheritdoc/>
  protected override Task OnAfterRenderAsync(bool firstRender)
  {
    // if (!firstRender) return Task.CompletedTask;
    return Task.CompletedTask;
  }

  public string CreateUrl(string key)
  {
    var url = self.navigation.admin_url($"clients/client/{Client.Id}?group={key}");
    // var output = $"<a data-group='{key}' href='url'>";
    var output = $"<a data-group='{key}' href='url'>";
    return url;
  }
}
