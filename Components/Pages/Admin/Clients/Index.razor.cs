// <copyright file="Index.razor.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>


using Service.Framework.Core.AppHook;
using Service.Schemas.Ui.Entities;
using Task = System.Threading.Tasks.Task;

namespace Service.Components.Pages.Admin.Clients;

public class IndexRazor : AdminComponentBase
{
  public string Title = string.Empty;

  // public Root.Entities.Client Client = new();
  public string Group = string.Empty;
  public string ClassName = string.Empty;

  /// <inheritdoc/>
  protected override async Task OnAfterRenderAsync(bool firstRender)
  {
    await base.OnAfterRenderAsync(firstRender);
    if (!firstRender) return;
    var uuid = await LocalStorage.GetItemAsync<string>("uuid");
    // Client = new Root.Entities.Client();
    // ClassName = Client.Id == 0 ? "9" : "12";
    var uiHook = UiHook.Instance;
    var breadcrumb = new List<MenuItem>
    {
      new() { Title = "Home", Url = "/admin/dashboards" },
      new() { Title = "Clients" }
    };
    await uiHook.CallAsync("update_toolbar", breadcrumb);
  }
}
