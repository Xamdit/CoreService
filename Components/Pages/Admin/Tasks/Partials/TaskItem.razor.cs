// <copyright file="TaskItem.razor.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Components;

namespace Service.Components.Pages.Admin.Tasks.Partials;

public class TaskItemRazor : AdminComponentBase
{
  [Parameter] public string SubscriptionName { get; set; } = "SubscriptionName";
  [Parameter] public string Customer { get; set; } = "Customer";
  [Parameter] public string Project { get; set; } = "Project";
  [Parameter] public string Status { get; set; } = "Status";
  [Parameter] public string NextBillingCycle { get; set; } = "NextBillingCycle";
  [Parameter] public string DateSubscribe { get; set; } = "DateSubscribe";
  [Parameter] public string LastSend { get; set; } = "DateEnd";
}
