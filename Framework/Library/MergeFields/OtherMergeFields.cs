using Service.Framework.Core.Engine;

namespace Service.Framework.Library.MergeFields;

using System;
using System.Collections.Generic;
using System.Linq;

public class OtherMergeFields(IServiceProvider serviceProvider) : AppMergeFields(serviceProvider)
{
  public override List<MergeField> Build()
  {
    var availableFor = new List<string>
    {
      "ticket",
      "client",
      "staff",
      "invoice",
      "estimate",
      "contract",
      "tasks",
      "proposals",
      "project",
      "leads",
      "credit_note",
      "subscriptions",
      "gdpr"
    };

    // Apply filters (similar to hooks)
    availableFor = ApplyFilters("other_merge_fields_available_for", availableFor);

    return new List<MergeField>
    {
      new() { Name = "Logo URL", Key = "{logo_url}", FromOptions = true, Available = availableFor },
      new() { Name = "Logo image with URL", Key = "{logo_image_with_url}", FromOptions = true, Available = availableFor },
      new() { Name = "Dark logo image with URL", Key = "{dark_logo_image_with_url}", FromOptions = true, Available = availableFor },
      new() { Name = "CRM URL", Key = "{crm_url}", FromOptions = true, Available = availableFor },
      new() { Name = "Admin URL", Key = "{admin_url}", FromOptions = true, Available = availableFor },
      new() { Name = "Main Domain", Key = "{main_domain}", FromOptions = true, Available = availableFor },
      new() { Name = "Company Name", Key = "{companyname}", FromOptions = true, Available = availableFor },
      new() { Name = "Email Signature", Key = "{email_signature}", FromOptions = true, Available = availableFor },
      new() { Name = "Terms & Conditions URL", Key = "{terms_and_conditions_url}", FromOptions = true, Available = availableFor },
      new() { Name = "Privacy Policy URL", Key = "{privacy_policy_url}", FromOptions = true, Available = availableFor }
    };
  }

  public Dictionary<string, string> format()
  {
    var fields = new Dictionary<string, string>
    {
      { "{logo_url}", GetBaseUrl() + "/uploads/company/" + GetOption("company_logo") }
    };

    var logoWidth = ApplyFilters("merge_field_logo_img_width", string.Empty);

    fields["{logo_image_with_url}"] =
      $"<a href=\"{GetSiteUrl()}\" target=\"_blank\">" +
      $"<img src=\"{GetBaseUrl()}/uploads/company/{GetOption("company_logo")}\"" +
      $"{(string.IsNullOrEmpty(logoWidth) ? string.Empty : $" width=\"{logoWidth}\"")}>" +
      $"</a>";

    if (!string.IsNullOrEmpty(GetOption("company_logo_dark")))
      fields["{dark_logo_image_with_url}"] =
        $"<a href=\"{GetSiteUrl()}\" target=\"_blank\">" +
        $"<img src=\"{GetBaseUrl()}/uploads/company/{GetOption("company_logo_dark")}\"" +
        $"{(string.IsNullOrEmpty(logoWidth) ? string.Empty : $" width=\"{logoWidth}\"")}>" +
        $"</a>";
    else
      fields["{dark_logo_image_with_url}"] = string.Empty;

    fields["{crm_url}"] = GetSiteUrl();
    fields["{admin_url}"] = GetAdminUrl();
    fields["{main_domain}"] = GetOption("main_domain");
    fields["{companyname}"] = GetOption("companyname");

    if (!IsStaffLoggedIn() || IsClientLoggedIn())
    {
      fields["{email_signature}"] = GetOption("email_signature");
    }
    else
    {
      var signature = GetStaffEmailSignature();
      fields["{email_signature}"] = string.IsNullOrEmpty(signature)
        ? GetOption("email_signature")
        : signature;
    }

    fields["{terms_and_conditions_url}"] = GetTermsUrl();
    fields["{privacy_policy_url}"] = GetPrivacyPolicyUrl();

    return ApplyFilters("other_merge_fields", fields);
  }

  // Mock implementations of helper methods
  private string GetBaseUrl()
  {
    return "https://example.com";
  }

  private string GetSiteUrl()
  {
    return "https://example.com/site";
  }

  private string GetAdminUrl()
  {
    return "https://example.com/admin";
  }

  private string GetOption(string key)
  {
    return "MockedOptionValue";
  }

  private bool IsStaffLoggedIn()
  {
    return true;
  }

  private bool IsClientLoggedIn()
  {
    return false;
  }

  private string GetStaffEmailSignature()
  {
    return "StaffSignature";
  }

  private string GetTermsUrl()
  {
    return "https://example.com/terms";
  }

  private string GetPrivacyPolicyUrl()
  {
    return "https://example.com/privacy";
  }

  private T ApplyFilters<T>(string filterName, T defaultValue)
  {
    // Mocked implementation of a filter mechanism (replace with actual logic)
    return defaultValue;
  }
}

public static class OtherMergeFieldsExtension
{
  public static OtherMergeFields other_merge_fields(this LibraryBase libs, IServiceProvider serviceProvider)
  {
    return new OtherMergeFields(serviceProvider);
  }
}
