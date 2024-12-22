// <copyright file="Client.razor.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Service.Core.Engine;

namespace Service.Components.Pages.Admin.Clients;

public class ClientRazor : MyComponentBase
{
  public string Title = string.Empty;
  public Client Client = new();
  public string Group = string.Empty;
  public string ClassName = string.Empty;
}
