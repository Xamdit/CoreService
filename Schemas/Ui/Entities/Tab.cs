// <copyright file="Tab.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Service.Schemas.Ui.Entities;

public class Tab
{
  public string Icon = string.Empty;
  public string Name = string.Empty;
  public string Slug { get; set; }
  public List<Tab> Children { get; set; } = new();
  public bool IsCollapse { get; set; }
}
