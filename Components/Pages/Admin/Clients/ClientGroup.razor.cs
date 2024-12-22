// <copyright file="ClientGroup.razor.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Components;
using Service.Core.Engine;

namespace Service.Components.Pages.Admin.Clients;

public class ClientGroupRazor : MyComponentBase
{
  [Parameter] public string Name { get; set; }

  public void OnSubmit()
  {
    // form_open('admin/clients/group',array('id'=>'customer-group-modal'));
  }

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
