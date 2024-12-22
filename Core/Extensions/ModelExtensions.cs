using Service.Framework.Entities;
using Service.Models;
using Service.Models.Client;
using Service.Models.Contracts;
using Service.Models.CreditNotes;
using Service.Models.Dashboards;
using Service.Models.Estimates;
using Service.Models.Gdpr;
using Service.Models.Invoices;
using Service.Models.KnowedgeBases;
using Service.Models.Leads;
using Service.Models.Misc;
using Service.Models.Payments;
using Service.Models.Projects;
using Service.Models.Proposals;
using Service.Models.Reports;
using Service.Models.Settings;
using Service.Models.Statements;
using Service.Models.Tasks;
using Service.Models.Tickets;
using Service.Models.Users;

namespace Service.Core.Extensions;

public static class ModelExtensions
{
  public static AnnouncementsModel announcements_model(this ModelBase model)
  {
    var (self, db) = getInstance();
    var output = new AnnouncementsModel(self, db);
    return output;
  }

  public static AuthModel auth_model(this ModelBase model)
  {
    var (self, db) = getInstance();
    var output = new AuthModel(self, db);
    return output;
  }

  public static DepartmentsModel departments_model(this ModelBase model)
  {
    var (self, db) = getInstance();
    var output = new DepartmentsModel(self, db);
    return output;
  }

  public static StaffModel staff_model(this ModelBase model)
  {
    var (self, db) = getInstance();
    var output = new StaffModel(self, db);
    return output;
  }

  public static UserAutoLoginModel user_autologin(this ModelBase model)
  {
    var (self, db) = getInstance();
    var output = new UserAutoLoginModel(self, db);
    return output;
  }

  public static AuthenticationModel authentication_model(this ModelBase model)
  {
    var (self, db) = getInstance();
    var output = new AuthenticationModel(self, db);
    return output;
  }

  public static TasksModel tasks_model(this ModelBase model)
  {
    var (self, db) = getInstance();
    var output = new TasksModel(self, db);
    return output;
  }

  public static ClientVaultEntriesModel client_vault_entries_model(this ModelBase model)
  {
    var (self, db) = getInstance();
    var output = new ClientVaultEntriesModel(self, db);
    return output;
  }

  public static CronModel cron_model(this ModelBase model)
  {
    var (self, db) = getInstance();
    var output = new CronModel(self, db);
    return output;
  }

  public static ClientGroupsModel client_groups_model(this ModelBase model)
  {
    var (self, db) = getInstance();
    var output = new ClientGroupsModel(self, db);
    return output;
  }

  public static StatementModel statement_model(this ModelBase model)
  {
    var (self, db) = getInstance();
    var output = new StatementModel(self, db);
    return output;
  }

  public static ClientsModel clients_model(this ModelBase model)
  {
    var (self, db) = getInstance();
    var output = new ClientsModel(self, db);
    return output;
  }

  public static CurrenciesModel currencies_model(this ModelBase model)
  {
    var (self, db) = getInstance();
    var output = new CurrenciesModel(self, db);
    return output;
  }

  public static SpamFiltersModel spam_filters_model(this ModelBase model)
  {
    var (self, db) = getInstance();
    var output = new SpamFiltersModel(self, db);
    return output;
  }

  public static InvoicesModel invoices_model(this ModelBase model)
  {
    var (self, db) = getInstance();
    var output = new InvoicesModel(self, db);
    return output;
  }

  public static PaymentsModel payments_model(this ModelBase model)
  {
    var (self, db) = getInstance();
    var output = new PaymentsModel(self, db);
    return output;
  }

  public static PaymentModesModel payment_modes_model(this ModelBase model)
  {
    var (self, db) = getInstance();
    var output = new PaymentModesModel(self, db);
    return output;
  }

  public static CreditNotesModel credit_notes_model(this ModelBase model)
  {
    var (self, db) = getInstance();
    var output = new CreditNotesModel(self, db);
    return output;
  }

  public static EstimateRequestModel estimate_request_model(this ModelBase model)
  {
    var (self, db) = getInstance();
    var output = new EstimateRequestModel(self, db);
    return output;
  }

  public static ProjectsModel projects_model(this ModelBase model)
  {
    var (self, db) = getInstance();
    var output = new ProjectsModel(self, db);
    return output;
  }

  public static RolesModel roles_model(this ModelBase model)
  {
    var (self, db) = getInstance();
    var output = new RolesModel(self, db);
    return output;
  }

  public static ExpensesModel expenses_model(this ModelBase model)
  {
    var (self, db) = getInstance();
    var output = new ExpensesModel(self, db);
    return output;
  }

  public static EmailsModel emails_model(this ModelBase model)
  {
    var (self, db) = getInstance();
    var output = new EmailsModel(self, db);
    return output;
  }

  public static DashboardModel dashboard_model(this ModelBase model)
  {
    var (self, db) = getInstance();
    var output = new DashboardModel(self, db);
    return output;
  }

  public static LeadsModel leads_model(this ModelBase model)
  {
    var (self, db) = getInstance();
    var output = new LeadsModel(self, db);
    return output;
  }

  public static MiscModel misc_model(this ModelBase model)
  {
    var (self, db) = getInstance();
    var output = new MiscModel(self, db);
    return output;
  }

  public static TicketsModel tickets_model(this ModelBase model)
  {
    var (self, db) = getInstance();
    var output = new TicketsModel(self, db);
    return output;
  }

  public static CustomFieldsModel custom_fields_model(this ModelBase model)
  {
    var (self, db) = getInstance();
    var output = new CustomFieldsModel(self, db);
    return output;
  }

  public static EmailScheduleModel email_schedule_model(this ModelBase model)
  {
    var (self, db) = getInstance();
    var output = new EmailScheduleModel(self, db);
    return output;
  }

  public static EstimatesModel estimates_model(this ModelBase model)
  {
    var (self, db) = getInstance();
    var output = new EstimatesModel(self, db);
    return output;
  }

  public static SubscriptionsModel subscriptions_model(this ModelBase model)
  {
    var (self, db) = getInstance();
    var output = new SubscriptionsModel(self, db);
    return output;
  }

  public static ContractsModel contracts_model(this ModelBase model)
  {
    var (self, db) = getInstance();
    var output = new ContractsModel(self, db);
    return output;
  }

  public static ProposalsModel proposals_model(this ModelBase model)
  {
    var (self, db) = getInstance();
    var output = new ProposalsModel(self, db);
    return output;
  }

  public static GdprModel gdpr_model(this ModelBase model)
  {
    var (self, db) = getInstance();
    var output = new GdprModel(self, db);
    return output;
  }

  public static InvoiceItemsModel invoice_items_model(this ModelBase model)
  {
    var (self, db) = getInstance();
    var output = new InvoiceItemsModel(self, db);
    return output;
  }

  public static PaymentAttemptsModel payment_attempts_model(this ModelBase model)
  {
    var (self, db) = getInstance();
    var output = new PaymentAttemptsModel(self, db);
    return output;
  }

  public static KnowledgeBaseModel knowledge_base_model(this ModelBase model)
  {
    var (self, db) = getInstance();
    var output = new KnowledgeBaseModel(self, db);
    return output;
  }

  public static ContractTypesModel contract_types_model(this ModelBase model)
  {
    var (self, db) = getInstance();
    var output = new ContractTypesModel(self, db);
    return output;
  }

  public static ReportsModel reports_model(this ModelBase model)
  {
    var (self, db) = getInstance();
    var output = new ReportsModel(self, db);
    return output;
  }

  public static SettingsModel settings_model(this ModelBase model)
  {
    var (self, db) = getInstance();
    var output = new SettingsModel(self, db);
    return output;
  }

  public static NewsfeedModel newsfeed_model(this ModelBase model)
  {
    var (self, db) = getInstance();
    var output = new NewsfeedModel(self, db);
    return output;
  }

  public static TemplatesModel templates_model(this ModelBase model)
  {
    var (self, db) = getInstance();
    var output = new TemplatesModel(self, db);
    return output;
  }
}
