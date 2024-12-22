// <copyright file="List.razor.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Components;
using Service.Core.Engine;

namespace Service.Components.Pages.Admin.Clients.Components;

public class ListRazor : MyComponentBase
{
  [Parameter] public string Name { get; set; }
  public List<string> OddEvent = new() { "odd", "event" };

  /// <inheritdoc/>
  protected override void OnInitialized()
  {
    // LocalStorage.SetItem("name", "John Smith");
    // var name = LocalStorage.GetItem<string>("name");
    // Console.WriteLine("admin area check");
  }

  protected void OnAfterRenderAsync(bool firstRender)
  {
    if (!firstRender) return;
  }
}
