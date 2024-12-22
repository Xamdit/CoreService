// <copyright file="BillingAndShipping.razor.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Components;
using Service.Core.Engine;

namespace Service.Components.Pages.Admin.Clients.Components.Panels;

public class BillingAndShippingRazor : MyComponentBase
{
  [Parameter] public string Name { get; set; }
}
