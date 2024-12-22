// <copyright file="Index.razor.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>


using System.Dynamic;
using Service.Framework.Core.AppHook;
using Service.Schemas.Ui.Entities;
using Task = System.Threading.Tasks.Task;

namespace Service.Components.Pages.Admin.Tasks;

public class IndexRazor : AdminComponentBase
{
  public string Title = string.Empty;
  public dynamic Client { get; set; } = new ExpandoObject();
  public string Group = string.Empty;
  public string ClassName = string.Empty;

  /// <inheritdoc/>
  protected override async Task OnAfterRenderAsync(bool firstRender)
  {
    if (!firstRender) return;
    ClassName = Client.Id == 0 ? "9" : "12";
    var uiHook = UiHook.Instance;
    var breadcrumb = new List<MenuItem>
    {
      new() { Title = "Home", Url = "/admin/dashboards" },
      new() { Title = "Clients" }
    };
    await uiHook.CallAsync("update_toolbar", breadcrumb);
  }
}
