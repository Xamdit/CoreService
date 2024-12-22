// <copyright file="TabDto.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Service.Schemas.Ui;

public class TabDto
{
  public object Collapse { get; set; }
  public string Slug { get; set; }
  public List<TabDto> Children = new();
}
