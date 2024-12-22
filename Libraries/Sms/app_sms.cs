// <copyright file="app_sms.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Service.Libraries.Sms;

public class SmsGateway
{
  public string Id { get; set; }
  public List<Trigger> Options { get; set; } = new();
  public Func<string, string, bool> SendSms { get; set; }
  public string Info { get; set; } = string.Empty;
  public string Name { get; set; } = string.Empty;
}

public class Trigger
{
  public string Id { get; set; }
  public List<string> MergeFields { get; set; }
  public string Label { get; set; }
  public string Info { get; set; }
  public string Value { get; set; }
}
