using Microsoft.AspNetCore.Mvc;
using Service.Controllers.Core;
using Service.Core.Extensions;
using Service.Entities;
using Service.Framework;

namespace Service.Controllers;

[ApiController]
[Route("api/subscription")]
public class SubscriptionController(ILogger<SubscriptionController> logger, MyInstance self, MyContext db) : ClientControllerBase(logger, self, db)
{
  public override void Init()
  {
  }

  [HttpGet]
  public IActionResult index(string hash)
  {
    var subscriptions_model = self.subscriptions_model(db);
    // var stripe_subscriptions = self.stripe_subscriptions();
    // var subscription = subscriptions_model.get_by_hash(hash);
    // if (!subscription)
    //   return show_404();
    //
    // var language = db.load_client_language(subscription.clientid);
    // data.locale = db.get_locale_key(language);
    //
    // data.stripe_customer = false;
    // if (!empty(subscription.stripe_customer_id)) data.stripe_customer = this.stripe_subscriptions.get_customer_with_default_source(subscription.stripe_customer_id);
    //
    // var plan = this.stripe_subscriptions.get_plan(subscription.stripe_plan_id);
    //
    // dynamic upcomingInvoice = new ExpandoObject();
    // upcomingInvoice.total = plan.amount * subscription.quantity;
    // upcomingInvoice.subtotal = upcomingInvoice.total;
    //
    // if (!string.IsNullOrEmpty(subscription.tax_percent))
    // {
    //   var totalTax = upcomingInvoice.total * (subscription.tax_percent / 100);
    //   upcomingInvoice.total += totalTax;
    // }
    //
    // data.total = upcomingInvoice.total;
    // upcomingInvoice.tax_percent = subscription.tax_percent;
    // var product = this.stripe_subscriptions.get_product(plan.product);
    //
    // upcomingInvoice.lines = new stdClass();
    // upcomingInvoice.lines.data = [];
    //
    // upcomingInvoice.lines.data[] =
    // [
    //   "description" = product.name + " (".app_format_money(strcasecmp(plan.currency, "JPY") == 0 ? plan.amount : plan.amount / 100, strtoupper(subscription.currency_name)). " / ".plan.interval.")",
    //   "amount" = plan.amount * subscription.quantity,
    //   "quantity" = subscription.quantity
    // ];
    //
    // this.disableNavigation();
    // this.disableSubMenu();
    // data.child_invoices = this.subscriptions_model.get_child_invoices(subscription.id);
    // data.invoice = subscription_invoice_preview_data(subscription, upcomingInvoice);
    // // this.app_scripts.theme("sticky-js", "assets/plugins/sticky/sticky.js");
    // data.plan = plan;
    // data.subscription = subscription;
    // data.title = subscription.name;
    // data.hash = hash;
    // data.bodyclass = "subscriptionhtml";
    // data(data);
    // this.view("subscriptionhtml");
    // this.layout();
    return MakeResult(data);
  }

  [HttpPost("subscribe")]
  public IActionResult subscribe(string subscription_hash)
  {
    var subscriptions_model = self.subscriptions_model(db);
    // var subscription = this.subscriptions_model.get_by_hash(subscription_hash);
    // if (!subscription)
    //   return show_404();
    //
    // var email = self.input.post<string>("stripeEmail");
    //
    // var stripe_customer_id = subscription.stripe_customer_id;
    // var source = self.input.post<string>("stripeToken");
    // if (string.IsNullOrEmpty(stripe_customer_id))
    // {
    //   try
    //   {
    //     var customer = this.stripe_subscriptions.create_customer([
    //       "email" = email,
    //       "source" = source,
    //       "description" = subscription.company
    //     ]);
    //
    //     stripe_customer_id = customer.id;
    //
    //
    //     db.Clients
    //       .Where(x => x.Id == subscription.clientid)
    //       .Update(x => new Client { StripeId = stripe_customer_id });
    //     db.SaveChanges();
    //   }
    //   catch (Exception e)
    //   {
    //     Console.WriteLine(e.Message);
    //   }
    // }
    // else if (!string.IsNullOrEmpty(stripe_customer_id))
    // {
    //   // Perhaps had source and it"s deleted
    //   var customer = this.stripe_subscriptions.get_customer(stripe_customer_id);
    //   if (string.IsNullOrEmpty(customer.default_source))
    //   {
    //     customer.source = source;
    //     customer.save();
    //   }
    // }
    //
    // try
    // {
    //   var @params = [];
    //   @params["tax_percent"] = subscription.tax_percent;
    //   @params["metadata"] =
    //   [
    //     "pcrm-subscription-hash" = subscription.hash
    //   ];
    //   @params["items"] = new[]
    //   {
    //     new
    //     {
    //       plan = subscription.stripe_plan_id
    //     }
    //   };
    //
    //   var future = false;
    //   var updateFirstBillingDate = false;
    //   if (!string.IsNullOrEmpty(subscription.date))
    //   {
    //     var anchor = strtotime(subscription.date);
    //
    //     if (subscription.date <= date("Y-m-d"))
    //     {
    //       anchor = false;
    //       updateFirstBillingDate = date("Y-m-d");
    //     }
    //
    //     if (anchor)
    //     {
    //       @params["billing_cycle_anchor"] = anchor;
    //       @params["prorate"] = false;
    //       future = true;
    //     }
    //   }
    //
    //   if (subscription.quantity > 1) @params["items"][0]["quantity"] = subscription.quantity;
    //
    //   var stripeSubscription = this.stripe_subscriptions.subscribe(stripe_customer_id,  params);
    //
    //   var update =
    //   [
    //     stripe_subscription_id = stripeSubscription.id,
    //     date_subscribed = date("Y-m-d H:i:s")
    //   ];
    //
    //   if (future)
    //   {
    //     update["status"] = "future";
    //     if (anchor) update["next_billing_cycle"] = anchor;
    //   }
    //
    //   if (updateFirstBillingDate) update["date"] = updateFirstBillingDate;
    //
    //   this.subscriptions_model.update(subscription.id, update);
    //
    //   send_email_customer_subscribed_to_subscription_to_staff(subscription);
    //
    //   hooks.do_action("customer_subscribed_to_subscription", subscription);
    //
    //   set_alert("success", label("customer_successfully_subscribed_to_subscription", subscription.name));
    // }
    // catch (Exception e)
    // {
    //   set_alert("warning", e.Message);
    // }
    // return Redirect(_SERVER["HTTP_REFERER"]);
    return Ok();
  }
}
