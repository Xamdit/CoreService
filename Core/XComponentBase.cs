// <copyright file="MyComponentBase.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>


using System.Dynamic;
using System.Linq.Expressions;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Service.Core.Engine;
using Service.Core.Extensions;
using Service.Entities;
using Service.Framework;
using Task = System.Threading.Tasks.Task;

namespace Service.Core;

public abstract class XComponentBase : MyComponentBase
{
  [Inject] public IJSRuntime Js { get; set; }

  [Inject] public ILocalStorageService LocalStorage { get; set; }
  [Inject] public SweetAlertService Swal { get; set; }
  [Inject] public IHttpContextAccessor HttpContextAccessor { get; set; }


  [Parameter] public string token { get; set; }

  public string? Env => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");


  public MyInstance self
  {
    get
    {
      var (self, db) = getInstance();
      self.navigation = NavigationManager;
      return self;
    }
  }

  public MyContext db
  {
    get
    {
      var (self, db) = getInstance();
      return db;
    }
  }

  protected MarkupString GetTemplatePart(string section, params dynamic[] args)
  {
    return new MarkupString(section);
  }

  public string Urlencode(string url)
  {
    return url;
  }

  public string D(DateTime? datetime)
  {
    return string.Empty;
  }


  protected MarkupString TermsUrl()
  {
    return new MarkupString(string.Empty);
  }

  protected string Get(string key)
  {
    var currentUrl = NavigationManager.Uri;
    var queryParams = currentUrl.Split('?').Skip(1).SelectMany(q => q.Split('&'));
    var token = queryParams.Select(param => param.Split('='))
      .FirstOrDefault(keyValue => keyValue.Length == 2 && keyValue[0] == key)?[1];
    return string.IsNullOrEmpty(token) ? string.Empty : token;
  }

  public string Route(string url)
  {
    // return self.Url.Route(url);
    return string.Empty;
  }

  public string? GetFragment()
  {
    var (self, db) = getInstance();
    self.navigation = NavigationManager;
    var uri = self.navigation.Uri;
    var fragment = NavigationManager.ToAbsoluteUri(uri).Fragment;
    if (string.IsNullOrEmpty(fragment)) return string.Empty;
    if (!fragment.Contains("#")) return string.Empty;
    var parts = fragment.Split('#').ToList();
    return parts.LastOrDefault();
  }

  public void GoTo(string path)
  {
    NavigationManager.NavigateTo(path);
  }

  public string Area(string url)
  {
    var section = NavigationManager.GetSection();
    var output = $"{section}/{url}";
    output = string.Join("/", output.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries));
    return output;
  }


  // public string app_format_money(decimal value, Currency? currency, bool b = false)
  public string App_format_money(decimal value, dynamic currency = default(ExpandoObject), bool b = false)
  {
    return string.Empty;
  }

  // public string app_format_number(decimal value, Currency currency)
  public string app_format_number(decimal value, dynamic currency)
  {
    return string.Empty;
  }

  public string Format_invoice_number(int id)
  {
    return string.Empty;
  }

  // public bool is_sale_discount_applied(Proposal item)
  public bool Is_sale_discount_applied(dynamic item)
  {
    return false;
  }

  public string Get_custom_field_value(int id, int subid, string value)
  {
    return string.Empty;
  }

  // public bool is_sale_discount(Proposal proposal, string fieldname)
  public bool Is_sale_discount(dynamic proposal, string fieldname)
  {
    return false;
  }

  public string Format_credit_note_number(int id)
  {
    return string.Empty;
  }

  public string Format_organization_info()
  {
    return string.Empty;
  }

  // public string format_customer_info(Contact contract, params string[] args)
  public string Format_customer_info(dynamic contract, params string[] args)
  {
    return string.Empty;
  }

  public string Get_custom_fields(string table, Expression<Func<object, bool>> condition)
  {
    return string.Empty;
  }

  public bool Is_client_logged_in()
  {
    return false;
  }

  public string Get_project_name_by_id(int id)
  {
    return string.Empty;
  }

  public string Clear_textarea_breaks(string? item = null)
  {
    return string.Empty;
  }

  public bool Is_empty_customer_company(int id)
  {
    return false;
  }

  public string Render_custom_fields<T>(string table, int id, Expression<Func<T, bool>> condition)
  {
    return string.Empty;
  }

  public string Get_upload_path_by_type(string path)
  {
    return string.Empty;
  }

  public string Optimize_dropbox_thumbnail(string path)
  {
    return string.Empty;
  }

  public string Get_mime_class(string mime)
  {
    return string.Empty;
  }

  /// <inheritdoc/>
  protected override async Task OnInitializedAsync()
  {
    var (self, db) = getInstance();
    await base.OnInitializedAsync();
    var httpContext = HttpContextAccessor.HttpContext;
    self.context = httpContext;
  }

  public bool is_admin()
  {
    return false;
  }

  public string text(string key)
  {
    return key;
  }

  public async Task CheckKeyAndRedirect(string key, string redirectUrl)
  {
    var keyExists = await LocalStorage.ContainKeyAsync(key);
    if (keyExists) NavigationManager.NavigateTo(redirectUrl);
  }

  public string GetImageProfile()
  {
    var output = $"https://api.xamdit.com/users/profile/nothing";
    try
    {
      output = $"https://api.xamdit.com/users/profile/{token}";
    }
    catch
    {
    }

    return output;
  }
}
