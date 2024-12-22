// <copyright file="MenuItem.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Service.Schemas.Ui.Entities;

public class MenuItem
{
  public string Icon { get; set; }
  public string Title { get; set; } = "Title";
  public string Description { get; set; } = string.Empty;
  public string Url { get; set; } = "#";
  public bool IsHeading { get; set; }
  public bool Accordion { get; set; }
  public List<MenuItem> Children { get; set; } = new();
  public string Active { get; set; } = "active";
}
