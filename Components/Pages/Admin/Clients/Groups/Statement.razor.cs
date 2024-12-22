// <copyright file="Statement.razor.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Components;
using Service.Core.Engine;

namespace Service.Components.Pages.Admin.Clients.Groups;

public class StatementRazor : MyComponentBase
{
  [Parameter] public string Name { get; set; }
  public string TmpBeginningBalance = string.Empty;
  public string From { get; set; }
  public string To { get; set; }
  public Dictionary<string, List<object>> statement = new();

  public void GenTmpBeginningBalance()
  {
    // var invoice = new Invoice();
    //    if (invoice.Id!=0)
    //    {
    //    // tmpBeginningBalance = tmpBeginningBalance + invoice['invoice_amount'];
    //    }
    //    else if (invoice.['payment_id']))
    //    {
    //    tmpBeginningBalance = tmpBeginningBalance - data['payment_total'];
    //    }
    //    else if (isset(data['credit_note_id']))
    //    {
    //    tmpBeginningBalance = tmpBeginningBalance - data['credit_note_amount'];
    //    }
    //    else if (isset(data['credit_note_refund_id']))
    //    {
    //    tmpBeginningBalance = tmpBeginningBalance + data['refund_amount'];
    //    }
    //    if (!isset(data['credit_id']))
    //    {
    //    app_format_money(tmpBeginningBalance, statement['currency'], true)
    //    }
  }

  /// <inheritdoc/>
  protected override void OnInitialized()
  {
    // LocalStorage.SetItem("name", "John Smith");
    // var name = LocalStorage.GetItem<string>("name");
    // Console.WriteLine("admin area check");
  }

  protected void OnAfterRenderAsync(bool firstRender)
  {
    if (!firstRender) return;
    statement["result"] = new List<object>();
    GenTmpBeginningBalance();
  }
}
