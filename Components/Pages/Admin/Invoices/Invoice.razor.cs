// <copyright file="Invoice.razor.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Service.Core.Engine;
using Task = System.Threading.Tasks.Task;

namespace Service.Components.Pages.Admin.Invoices;

public class InvoiceRazor : MyComponentBase
{
  public string Title = string.Empty;

  // public Root.Entities.Client Client = new();
  public string Group = string.Empty;
  public string ClassName = string.Empty;

  /// <inheritdoc/>
  protected override async Task OnAfterRenderAsync(bool firstRender)
  {
    if (!firstRender) return;
  }
}
