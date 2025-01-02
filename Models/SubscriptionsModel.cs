using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Service.Core.Extensions;
using Service.Entities;
using Service.Framework;
using Service.Framework.Core.Extensions;
using Service.Helpers;
using Service.Models.Invoices;

namespace Service.Models;

public class SubscriptionsModel(MyInstance self, MyContext db) : MyModel(self,db)
{
  private readonly InvoicesModel invoices_model = self.invoices_model(db);

  public List<Subscription> get(Expression<Func<Subscription, bool>> where)
  {
    var query = db.Subscriptions.AsQueryable();
    query = join(query);
    query = join(query);
    var rows = query.Where(where).ToList();
    return rows;
  }

  public Subscription get_by_id(int id, Expression<Func<Subscription, bool>> where)
  {
    var query = db.Subscriptions.AsQueryable();
    // query = select(query);
    query = join(query);
    query = query.Where(s => s.Id == id);
    return query.FirstOrDefault(where);
  }

  public Subscription? get_by_hash(string hash, Expression<Func<Subscription, bool>> where)
  {
    var query = db.Subscriptions.AsQueryable();
    query = join(query);
    query = query.Where(s => s.Hash == hash);
    return query.FirstOrDefault(where);
  }

  public List<Invoice> get_child_invoices(int id)
  {
    var invoiceIds = db.Invoices
      .Where(i => i.SubscriptionId == id)
      .Select(i => i.Id)
      .ToList();

    var childInvoices = invoiceIds
      .Select(invoices_model.get)
      .Where(x => x != null)
      .ToList();

    return childInvoices;
  }

  public int create(Dictionary<string, object> data)
  {
    data = handleSelectedTax(data);
    var subscription = new Subscription
    {
      // Assign values from the data dictionary
      DateCreated = DateTime.Now,
      Hash = self.helper.uuid(),
      CreatedFrom = staff_user_id
    };

    db.Subscriptions.Add(subscription);
    db.SaveChanges();

    return subscription.Id;
  }

  public bool update(int id, Dictionary<string, object> data)
  {
    data = handleSelectedTax(data);

    var subscription = db.Subscriptions.Find(id);
    if (subscription == null) return false;

    // Update the subscription entity with data
    db.Entry(subscription).CurrentValues.SetValues(data);
    db.SaveChanges();

    return true;
  }

  public bool send_email_template(int id, string cc = "", string template = "subscription_send_to_customer")
  {
    var subscription = get_by_id(id, x => true);
    var contact = db.Clients
      .FirstOrDefault(c => c.Id == self.helper.get_primary_contact_user_id(subscription.ClientId));

    if (contact == null) return false;

    var sent = db.send_mail_template(template, subscription, contact, cc);
    if (!sent || template != "subscription_send_to_customer") return false;
    subscription.LastSentAt = DateTime.UtcNow;
    db.Subscriptions.Update(subscription);
    db.SaveChanges();
    return true;
  }

  public Subscription delete(int id, bool simpleDelete = false)
  {
    var subscription = get_by_id(id, x => true);
    if (subscription.InTestEnvironment == null && !string.IsNullOrEmpty(subscription.StripeSubscriptionId) && simpleDelete == false) return null;
    var result = db.Subscriptions.Where(x => x.Id == subscription.Id).Delete();
    if (subscription == null) return null;
    // Reset the subscription_id on associated invoices

    db.Invoices.Where(i => i.SubscriptionId == id)
      .Update(x => new Invoice { SubscriptionId = 0 });

    return subscription;
  }

  private IQueryable<Subscription> select(IQueryable<Subscription> query)
  {
    return query.Select(s => new Subscription
    {
      // Specify the fields to select, example:
      Id = s.Id,
      ProjectId = s.ProjectId,
      ClientId = s.ClientId,
      Name = s.Name,
      CreatedFrom = s.CreatedFrom
      // Currency.Name = s.Currency.Name
      // Add more fields as necessary...
    });
  }

  private IQueryable<Subscription> join(IQueryable<Subscription> query)
  {
    return query
      .Include(x => x.Currency)
      .Include(x => x.Tax)
      .Include(x => x.Invoices)
      .Include(x => x.Client);
  }

  protected Dictionary<string, object> handleSelectedTax(Dictionary<string, object> data)
  {
    foreach (var key in new[] { "stripe_tax_id", "stripe_tax_id_2" })
    {
      var localKey = key == "stripe_tax_id" ? "tax_id" : "tax_id_2";

      if (data.ContainsKey(key) && data[key] != null && !string.IsNullOrEmpty(data[key].ToString()))
      {
        // var stripeTax = RetrieveStripeTaxRate(data[key].ToString());
        var displayName = "stripeTax.DisplayName";
        // if (!string.IsNullOrEmpty(stripeTax.Jurisdiction)) displayName += " - " + stripeTax.Jurisdiction;
        var dbTax = db.Taxes
          .FirstOrDefault(t =>
              t.Name == displayName
            // && t.TaxRate == stripeTax.Percentage
          );
        if (dbTax == null)
        {
          // var newTax = new Taxis { Name = displayName, TaxRate = stripeTax.Percentage };
          // db.Taxes.Add(newTax);
          // db.SaveChanges();
          // data[localKey] = newTax.Id;
        }
        else
        {
          data[localKey] = dbTax.Id;
        }
      }
      else if (data.ContainsKey(key) && string.IsNullOrEmpty(data[key]?.ToString()))
      {
        data[localKey] = 0;
        data[key] = null;
      }
    }

    return data;
  }
}
