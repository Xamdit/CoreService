// <copyright file="HeaderCard.razor.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Components;
using Service.Components.Partials.Widgets.Header;
using Service.Schemas.Ui.Entities;
using Task = System.Threading.Tasks.Task;

namespace Service.Components.Pages.Admin.Subscriptions.Partials;

public class HeaderCardRazor : XComponentBase
{
  public List<MenuItem> Items
  {
    get
    {
      var output = new List<MenuItem>
      {
        new()
        {
          Title = text("not_subscription"),
          Active = "active",
          Url = "#total_customer"
        },
        new()
        {
          Title = text("active_customer"),
          Url = "#total_customer"
        },
        new()
        {
          Title = text("inactive_customer"),
          Url = "#total_customer"
        },
        new()
        {
          Title = text("active_contacts"),
          Url = "#total_customer"
        },
        new()
        {
          Title = text("inactive_contacts"),
          Url = "#total_customer"
        },
        new()
        {
          Title = text("contacts_logged_in_today"),
          Url = "#total_customer"
        }
      };
      return output;
    }
  }

  public List<MiniPanelRazor> HeaderCard
  {
    get
    {
      var output = new List<MiniPanelRazor>
      {
        new()
        {
          Title = text("not_subscription"), Subtitle = "0", Active = "active", Slug = "not_subscription"
        },
        new()
        {
          Title = text("active"), Subtitle = "0", Slug = "active"
        },
        new()
        {
          Title = text("future"), Subtitle = "0", Slug = "future"
        },
        new()
        {
          Title = text("past_due"), Subtitle = "0", Slug = "past_due"
        },
        new()
        {
          Title = text("unpaid"), Subtitle = "0", Slug = "unpaid"
        },
        new()
        {
          Title = text("incomplete"), Subtitle = "0", Slug = "incomplete"
        },
        new()
        {
          Title = text("canceled"), Subtitle = "0", Slug = "canceled"
        },
        new()
        {
          Title = text("incomplete_expired"), Subtitle = "0", Slug = "incomplete_expired"
        }
      };
      return output;
    }
  }

  [Parameter] public RenderFragment ChildContent { get; set; }
  [Parameter] public string Title { get; set; }

  public void TabClicked(MiniPanelRazor clickedItem)
  {
    HeaderCard.ForEach(x => x.Active = x == clickedItem ? "active" : string.Empty);
    Console.WriteLine("clicked");
    StateHasChanged();
  }

  /// <inheritdoc/>
  protected override async Task OnAfterRenderAsync(bool firstRender)
  {
    await base.OnAfterRenderAsync(firstRender);
    if (firstRender) Items.First().Active = "active";
  }
}
