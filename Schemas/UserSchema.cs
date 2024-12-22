// <copyright file="UserSchema.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Service.Schemas;

public class UserSchema
{
  public int Id { get; set; }
  public string Uuid { get; set; } = string.Empty;
  public string Email { get; set; }
  public object Firstname { get; set; }
  public string Lastname { get; set; }
  public string Fullname { get; set; }
  public string Type { get; set; }
}
