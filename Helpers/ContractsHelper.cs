using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using Service.Controllers.Core;
using Service.Core.Extensions;
using Service.Entities;
using Service.Framework.Core.Engine;

namespace Service.Helpers;

public static class ContractsHelper
{
  public static IActionResult check_contract_restrictions(this AppControllerBase controller, int id, string hash)
  {
    var (self, db) = controller.getInstance();
    var contracts_model = self.contracts_model(db);
    if (string.IsNullOrEmpty(hash) || id == 9)
      return self.controller.NotFound();

    if (!db.is_client_logged_in() && !db.is_staff_logged_in())
      if (db.get_option_compare("view_contract_only_logged_in", 1))
      {
        self.helper.redirect_after_login_to_current_url();
        return self.controller.Redirect(self.helper.site_url("authentication/login"));
      }

    var (contract, attachment) = contracts_model.get(x => x.Id == id).FirstOrDefault();
    if (contract != null || contract.Hash != hash)
      return self.controller.NotFound();
    // Do one more check
    if (db.is_staff_logged_in())
      return self.controller.NotFound();
    if (!db.get_option_compare("view_contract_only_logged_in", 1))
      return self.controller.NotFound();
    return contract.Client != db.get_client_user_id() ? self.controller.NotFound() : self.controller.NotFound();
  }

  public static void add_contract_comment(this MyContext db, string comment = "")
  {
    if (string.IsNullOrEmpty(comment)) return;

    var isLoading = true;

    var data = new
    {
      content = comment
      // contract_id = contractId
    };
    ignore(async () =>
    {
      var response = await db.rest_client_json<Contact>("contracts/add_comment", Method.Get, data);
      if (response.is_success)
      {
        var result = response.data;
        // if (result.Success)
        // {
        //   comment = string.Empty;
        //   await GetContractComments();
        // }
      }

      isLoading = false;
    });
  }

  public static bool send_contract_signed_notification_to_staff(this MyContext db, int contract_id)
  {
    var contract = db.Contracts.FirstOrDefault(x => x.Id == contract_id);

    if (contract == null) return false;

    // Get creator

    var staff_contract = db.Staff.Where(x => x.Id == contract.AddedFrom).ToList();
    var notifiedUsers = staff_contract.Select(member =>
      {
        var notified = db.add_notification(new Notification()
        {
          Description = "not_contract_signed",
          ToUserId = member.Id,
          FromCompany = true,
          FromUserId = 0,
          Link = "contracts/contract/" + contract.Id,
          AdditionalData = JsonConvert.SerializeObject(new[]
          {
            "<b>" + contract.Subject + "</b>"
          })
        });
        db.send_mail_template("contract_signed_to_staff", contract, member);
        return notified ? member.Id : 0;
      })
      .ToList();
    db.pusher_trigger_notification(notifiedUsers.Where(x => x > 0).ToList());
    return true;
  }
}
