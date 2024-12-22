// <copyright file="ClientHeaderCard.razor.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Components;
using Service.Schemas.Ui.Entities;
using Task = System.Threading.Tasks.Task;

namespace Service.Components.Pages.Admin.Clients.Components;

public class ClientHeaderCardRazor : XComponentBase
{
  public List<MenuItem> Items => new()
  {
    new MenuItem
    {
      Title = text("total_customer"),
      Active = "active",
      Url = "#total_customer"
    },
    new MenuItem
    {
      Title = text("active_customer"),
      Url = "#total_customer"
    },
    new MenuItem
    {
      Title = text("inactive_customer"),
      Url = "#total_customer"
    },
    new MenuItem
    {
      Title = text("active_contacts"),
      Url = "#total_customer"
    },
    new MenuItem
    {
      Title = text("inactive_contacts"),
      Url = "#total_customer"
    },
    new MenuItem
    {
      Title = text("contacts_logged_in_today"),
      Url = "#total_customer"
    }
  };

  [Parameter] public RenderFragment ChildContent { get; set; }

  public void TabClicked(MenuItem clickedItem)
  {
    Items.ForEach(x => x.Active = x == clickedItem ? "active" : string.Empty);
    Console.WriteLine("clicked");
  }

  /// <inheritdoc/>
  protected override async Task OnAfterRenderAsync(bool firstRender)
  {
    await base.OnAfterRenderAsync(firstRender);
    if (firstRender) Items.First().Active = "active";
  }
}
