// <copyright file="Filter.razor.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Service.Core.Engine;
using Task = System.Threading.Tasks.Task;

namespace Service.Components.Pages.Admin.Clients.Components;

public class FilterRazor : MyComponentBase
{
  public string Title = string.Empty;

  // public Client Client = new();
  public string Group = string.Empty;
  public string ClassName = string.Empty;

  /// <inheritdoc/>
  protected override Task OnAfterRenderAsync(bool firstRender)
  {
    if (!firstRender) return Task.CompletedTask;
    // Client = new Client();
    // ClassName = Client.Id == 0 ? "9" : "12";
    return Task.CompletedTask;
  }
}
