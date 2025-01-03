using Microsoft.EntityFrameworkCore;

namespace Service.Entities;

public partial class MyContext : DbContext
{
    public MyContext()
    {
    }

    public MyContext(DbContextOptions<MyContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ActivityLog> ActivityLogs { get; set; }

    public virtual DbSet<Announcement> Announcements { get; set; }

    public virtual DbSet<Client> Clients { get; set; }

    public virtual DbSet<ColumnInfo> ColumnInfos { get; set; }

    public virtual DbSet<Consent> Consents { get; set; }

    public virtual DbSet<ConsentPurpose> ConsentPurposes { get; set; }

    public virtual DbSet<Contact> Contacts { get; set; }

    public virtual DbSet<ContactPermission> ContactPermissions { get; set; }

    public virtual DbSet<Contract> Contracts { get; set; }

    public virtual DbSet<ContractComment> ContractComments { get; set; }

    public virtual DbSet<ContractRenewal> ContractRenewals { get; set; }

    public virtual DbSet<ContractsType> ContractsTypes { get; set; }

    public virtual DbSet<Country> Countries { get; set; }

    public virtual DbSet<Credit> Credits { get; set; }

    public virtual DbSet<CreditNote> CreditNotes { get; set; }

    public virtual DbSet<CreditNoteRefund> CreditNoteRefunds { get; set; }

    public virtual DbSet<Currency> Currencies { get; set; }

    public virtual DbSet<CustomField> CustomFields { get; set; }

    public virtual DbSet<CustomFieldsValue> CustomFieldsValues { get; set; }

    public virtual DbSet<CustomerAdmin> CustomerAdmins { get; set; }

    public virtual DbSet<CustomerGroup> CustomerGroups { get; set; }

    public virtual DbSet<CustomersGroup> CustomersGroups { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<DismissedAnnouncement> DismissedAnnouncements { get; set; }

    public virtual DbSet<EmailTemplate> EmailTemplates { get; set; }

    public virtual DbSet<Estimate> Estimates { get; set; }

    public virtual DbSet<EstimateRequest> EstimateRequests { get; set; }

    public virtual DbSet<EstimateRequestForm> EstimateRequestForms { get; set; }

    public virtual DbSet<EstimateRequestStatus> EstimateRequestStatuses { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<Expense> Expenses { get; set; }

    public virtual DbSet<ExpensesCategory> ExpensesCategories { get; set; }

    public virtual DbSet<File> Files { get; set; }

    public virtual DbSet<FormQuestion> FormQuestions { get; set; }

    public virtual DbSet<FormQuestionBox> FormQuestionBoxes { get; set; }

    public virtual DbSet<FormQuestionBoxDescription> FormQuestionBoxDescriptions { get; set; }

    public virtual DbSet<FormResult> FormResults { get; set; }

    public virtual DbSet<GdprRequest> GdprRequests { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<InvoicePaymentRecord> InvoicePaymentRecords { get; set; }

    public virtual DbSet<Item> Items { get; set; }

    public virtual DbSet<ItemTax> ItemTaxes { get; set; }

    public virtual DbSet<Itemable> Itemables { get; set; }

    public virtual DbSet<ItemsGroup> ItemsGroups { get; set; }

    public virtual DbSet<KnowedgeBaseArticleFeedback> KnowedgeBaseArticleFeedbacks { get; set; }

    public virtual DbSet<KnowledgeBase> KnowledgeBases { get; set; }

    public virtual DbSet<KnowledgeBaseGroup> KnowledgeBaseGroups { get; set; }

    public virtual DbSet<Lead> Leads { get; set; }

    public virtual DbSet<LeadActivityLog> LeadActivityLogs { get; set; }

    public virtual DbSet<LeadIntegrationEmail> LeadIntegrationEmails { get; set; }

    public virtual DbSet<LeadsEmailIntegration> LeadsEmailIntegrations { get; set; }

    public virtual DbSet<LeadsSource> LeadsSources { get; set; }

    public virtual DbSet<LeadsStatus> LeadsStatuses { get; set; }

    public virtual DbSet<MailQueue> MailQueues { get; set; }

    public virtual DbSet<Migration> Migrations { get; set; }

    public virtual DbSet<Milestone> Milestones { get; set; }

    public virtual DbSet<Module> Modules { get; set; }

    public virtual DbSet<NewsfeedCommentLike> NewsfeedCommentLikes { get; set; }

    public virtual DbSet<NewsfeedPost> NewsfeedPosts { get; set; }

    public virtual DbSet<NewsfeedPostComment> NewsfeedPostComments { get; set; }

    public virtual DbSet<NewsfeedPostLike> NewsfeedPostLikes { get; set; }

    public virtual DbSet<Note> Notes { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Option> Options { get; set; }

    public virtual DbSet<PaymentAttempt> PaymentAttempts { get; set; }

    public virtual DbSet<PaymentMode> PaymentModes { get; set; }

    public virtual DbSet<PinnedProject> PinnedProjects { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<ProjectActivity> ProjectActivities { get; set; }

    public virtual DbSet<ProjectDiscussion> ProjectDiscussions { get; set; }

    public virtual DbSet<ProjectDiscussionComment> ProjectDiscussionComments { get; set; }

    public virtual DbSet<ProjectFile> ProjectFiles { get; set; }

    public virtual DbSet<ProjectMember> ProjectMembers { get; set; }

    public virtual DbSet<ProjectNote> ProjectNotes { get; set; }

    public virtual DbSet<ProjectSetting> ProjectSettings { get; set; }

    public virtual DbSet<Proposal> Proposals { get; set; }

    public virtual DbSet<ProposalComment> ProposalComments { get; set; }

    public virtual DbSet<RelatedItem> RelatedItems { get; set; }

    public virtual DbSet<Reminder> Reminders { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<SalesActivity> SalesActivities { get; set; }

    public virtual DbSet<ScheduledEmail> ScheduledEmails { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<Session> Sessions { get; set; }

    public virtual DbSet<SharedCustomerFile> SharedCustomerFiles { get; set; }

    public virtual DbSet<SpamFilter> SpamFilters { get; set; }

    public virtual DbSet<Staff> Staff { get; set; }

    public virtual DbSet<StaffDepartment> StaffDepartments { get; set; }

    public virtual DbSet<StaffPermission> StaffPermissions { get; set; }

    public virtual DbSet<Subscription> Subscriptions { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<Taggable> Taggables { get; set; }

    public virtual DbSet<Task> Tasks { get; set; }

    public virtual DbSet<TaskAssigned> TaskAssigneds { get; set; }

    public virtual DbSet<TaskChecklistItem> TaskChecklistItems { get; set; }

    public virtual DbSet<TaskComment> TaskComments { get; set; }

    public virtual DbSet<TaskFollower> TaskFollowers { get; set; }

    public virtual DbSet<TasksChecklistTemplate> TasksChecklistTemplates { get; set; }

    public virtual DbSet<TasksTimer> TasksTimers { get; set; }

    public virtual DbSet<Taxis> Taxes { get; set; }

    public virtual DbSet<Template> Templates { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<TicketAttachment> TicketAttachments { get; set; }

    public virtual DbSet<TicketReply> TicketReplies { get; set; }

    public virtual DbSet<TicketsPipeLog> TicketsPipeLogs { get; set; }

    public virtual DbSet<TicketsPredefinedReply> TicketsPredefinedReplies { get; set; }

    public virtual DbSet<TicketsPriority> TicketsPriorities { get; set; }

    public virtual DbSet<TicketsStatus> TicketsStatuses { get; set; }

    public virtual DbSet<Todo> Todos { get; set; }

    public virtual DbSet<TrackedMail> TrackedMails { get; set; }

    public virtual DbSet<TwoCheckoutLog> TwoCheckoutLogs { get; set; }

    public virtual DbSet<UserAutoLogin> UserAutoLogins { get; set; }

    public virtual DbSet<UserMetum> UserMeta { get; set; }

    public virtual DbSet<Vault> Vaults { get; set; }

    public virtual DbSet<ViewsTracking> ViewsTrackings { get; set; }

    public virtual DbSet<WebToLead> WebToLeads { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;user=root;password=password;database=crm;convert zero datetime=True;treattinyasboolean=True", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.3.0-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_general_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<ActivityLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("activity_log")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.StaffId, "staffid");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(3)")
                .HasColumnType("datetime(3)")
                .HasColumnName("date_created");
            entity.Property(e => e.Description)
                .HasMaxLength(191)
                .HasColumnName("description");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");

            entity.HasOne(d => d.Staff).WithMany(p => p.ActivityLogs)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("activity_log_staff_id_fkey");
        });

        modelBuilder.Entity<Announcement>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("announcements")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.StaffId, "announcements_staff_id_fkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(3)")
                .HasColumnType("datetime(3)")
                .HasColumnName("date_created");
            entity.Property(e => e.Message)
                .HasMaxLength(191)
                .HasColumnName("message");
            entity.Property(e => e.Name)
                .HasMaxLength(191)
                .HasColumnName("name");
            entity.Property(e => e.ShowName).HasColumnName("show_name");
            entity.Property(e => e.ShowToStaff).HasColumnName("show_to_staff");
            entity.Property(e => e.ShowToUsers).HasColumnName("show_to_users");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");

            entity.HasOne(d => d.Staff).WithMany(p => p.Announcements)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("announcements_staff_id_fkey");
        });

        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("clients")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.Active, "active");

            entity.HasIndex(e => e.Company, "company");

            entity.HasIndex(e => e.CountryId, "country");

            entity.HasIndex(e => e.LeadId, "leadid");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.AddedFrom).HasColumnName("added_from");
            entity.Property(e => e.Address)
                .HasMaxLength(191)
                .HasColumnName("address");
            entity.Property(e => e.BillingCity)
                .HasMaxLength(191)
                .HasColumnName("billing_city");
            entity.Property(e => e.BillingCountry).HasColumnName("billing_country");
            entity.Property(e => e.BillingState)
                .HasMaxLength(191)
                .HasColumnName("billing_state");
            entity.Property(e => e.BillingStreet)
                .HasMaxLength(191)
                .HasColumnName("billing_street");
            entity.Property(e => e.BillingZip)
                .HasMaxLength(191)
                .HasColumnName("billing_zip");
            entity.Property(e => e.City)
                .HasMaxLength(191)
                .HasColumnName("city");
            entity.Property(e => e.Company)
                .HasMaxLength(191)
                .HasColumnName("company");
            entity.Property(e => e.CountryId).HasColumnName("country_id");
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("date_created");
            entity.Property(e => e.DefaultCurrency).HasColumnName("default_currency");
            entity.Property(e => e.DefaultLanguage)
                .HasMaxLength(191)
                .HasColumnName("default_language");
            entity.Property(e => e.Latitude)
                .HasMaxLength(191)
                .HasColumnName("latitude");
            entity.Property(e => e.LeadId).HasColumnName("lead_id");
            entity.Property(e => e.Longitude)
                .HasMaxLength(191)
                .HasColumnName("longitude");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(191)
                .HasColumnName("phone_number");
            entity.Property(e => e.RegistrationConfirmed).HasColumnName("registration_confirmed");
            entity.Property(e => e.ShippingCity)
                .HasMaxLength(191)
                .HasColumnName("shipping_city");
            entity.Property(e => e.ShippingCountry).HasColumnName("shipping_country");
            entity.Property(e => e.ShippingState)
                .HasMaxLength(191)
                .HasColumnName("shipping_state");
            entity.Property(e => e.ShippingStreet)
                .HasMaxLength(191)
                .HasColumnName("shipping_street");
            entity.Property(e => e.ShippingZip)
                .HasMaxLength(191)
                .HasColumnName("shipping_zip");
            entity.Property(e => e.ShowPrimaryContact).HasColumnName("show_primary_contact");
            entity.Property(e => e.State)
                .HasMaxLength(191)
                .HasColumnName("state");
            entity.Property(e => e.StripeId)
                .HasMaxLength(191)
                .HasColumnName("stripe_id");
            entity.Property(e => e.Vat)
                .HasMaxLength(191)
                .HasColumnName("vat");
            entity.Property(e => e.Website)
                .HasMaxLength(191)
                .HasColumnName("website");
            entity.Property(e => e.Zip)
                .HasMaxLength(191)
                .HasColumnName("zip");

            entity.HasOne(d => d.Country).WithMany(p => p.Clients)
                .HasForeignKey(d => d.CountryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("clients_country_id_fkey");

            entity.HasOne(d => d.Lead).WithMany(p => p.Clients)
                .HasForeignKey(d => d.LeadId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("clients_lead_id_fkey");
        });

        modelBuilder.Entity<ColumnInfo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("column_info")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Default)
                .HasMaxLength(191)
                .HasColumnName("default");
            entity.Property(e => e.Extra)
                .HasMaxLength(191)
                .HasColumnName("extra");
            entity.Property(e => e.Field)
                .HasMaxLength(191)
                .HasColumnName("field");
            entity.Property(e => e.Key)
                .HasMaxLength(191)
                .HasColumnName("key");
            entity.Property(e => e.Null)
                .HasMaxLength(191)
                .HasColumnName("null");
            entity.Property(e => e.Type)
                .HasMaxLength(191)
                .HasColumnName("type");
        });

        modelBuilder.Entity<Consent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("consents")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.ContactId, "contact_id");

            entity.HasIndex(e => e.LeadId, "lead_id");

            entity.HasIndex(e => e.PurposeId, "purpose_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Action)
                .HasMaxLength(191)
                .HasColumnName("action");
            entity.Property(e => e.ContactId).HasColumnName("contact_id");
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(3)")
                .HasColumnType("datetime(3)")
                .HasColumnName("date_created");
            entity.Property(e => e.Description)
                .HasMaxLength(191)
                .HasColumnName("description");
            entity.Property(e => e.Ip)
                .HasMaxLength(191)
                .HasColumnName("ip");
            entity.Property(e => e.LeadId).HasColumnName("lead_id");
            entity.Property(e => e.OptInPurposeDescription)
                .HasMaxLength(191)
                .HasColumnName("opt_in_purpose_description");
            entity.Property(e => e.PurposeId).HasColumnName("purpose_id");
            entity.Property(e => e.StaffName)
                .HasMaxLength(191)
                .HasColumnName("staff_name");

            entity.HasOne(d => d.Contact).WithMany(p => p.Consents)
                .HasForeignKey(d => d.ContactId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("consents_contact_id_fkey");

            entity.HasOne(d => d.Lead).WithMany(p => p.Consents)
                .HasForeignKey(d => d.LeadId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("consents_lead_id_fkey");

            entity.HasOne(d => d.Purpose).WithMany(p => p.Consents)
                .HasForeignKey(d => d.PurposeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("consents_purpose_id_fkey");
        });

        modelBuilder.Entity<ConsentPurpose>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("consent_purposes")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(3)")
                .HasColumnType("datetime(3)")
                .HasColumnName("date_created");
            entity.Property(e => e.Description)
                .HasMaxLength(191)
                .HasColumnName("description");
            entity.Property(e => e.LastUpdated)
                .HasMaxLength(191)
                .HasColumnName("last_updated");
            entity.Property(e => e.Name)
                .HasMaxLength(191)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Contact>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("contacts")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.Email, "email");

            entity.HasIndex(e => e.FirstName, "first_name");

            entity.HasIndex(e => e.IsPrimary, "is_primary");

            entity.HasIndex(e => e.LastName, "last_name");

            entity.HasIndex(e => e.UserId, "user_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.ContractEmails).HasColumnName("contract_emails");
            entity.Property(e => e.CreditNoteEmails).HasColumnName("credit_note_emails");
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(3)")
                .HasColumnType("datetime(3)")
                .HasColumnName("date_created");
            entity.Property(e => e.Direction)
                .HasMaxLength(191)
                .HasColumnName("direction");
            entity.Property(e => e.Email)
                .HasMaxLength(191)
                .HasColumnName("email");
            entity.Property(e => e.EmailVerificationKey)
                .HasMaxLength(191)
                .HasColumnName("email_verification_key");
            entity.Property(e => e.EmailVerificationSentAt)
                .HasMaxLength(191)
                .HasColumnName("email_verification_sent_at");
            entity.Property(e => e.EmailVerifiedAt)
                .HasMaxLength(191)
                .HasColumnName("email_verified_at");
            entity.Property(e => e.EstimateEmails).HasColumnName("estimate_emails");
            entity.Property(e => e.FirstName)
                .HasMaxLength(191)
                .HasColumnName("first_name");
            entity.Property(e => e.InvoiceEmails).HasColumnName("invoice_emails");
            entity.Property(e => e.IsPrimary)
                .HasDefaultValueSql("'0'")
                .HasColumnName("is_primary");
            entity.Property(e => e.LastIp)
                .HasMaxLength(191)
                .HasColumnName("last_ip");
            entity.Property(e => e.LastLogin)
                .HasColumnType("datetime(3)")
                .HasColumnName("last_login");
            entity.Property(e => e.LastName)
                .HasMaxLength(191)
                .HasColumnName("last_name");
            entity.Property(e => e.LastPasswordChange)
                .HasColumnType("datetime(3)")
                .HasColumnName("last_password_change");
            entity.Property(e => e.NewPassKey)
                .HasMaxLength(191)
                .HasColumnName("new_pass_key");
            entity.Property(e => e.NewPassKeyRequested)
                .HasMaxLength(191)
                .HasColumnName("new_pass_key_requested");
            entity.Property(e => e.Password)
                .HasMaxLength(191)
                .HasColumnName("password");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(191)
                .HasColumnName("phone_number");
            entity.Property(e => e.ProfileImage)
                .HasMaxLength(191)
                .HasColumnName("profile_image");
            entity.Property(e => e.ProjectEmails).HasColumnName("project_emails");
            entity.Property(e => e.TaskEmails).HasColumnName("task_emails");
            entity.Property(e => e.TicketEmails).HasColumnName("ticket_emails");
            entity.Property(e => e.Title)
                .HasMaxLength(191)
                .HasColumnName("title");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Uuid)
                .HasMaxLength(36)
                .HasColumnName("uuid");
        });

        modelBuilder.Entity<ContactPermission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("contact_permissions")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.PermissionId).HasColumnName("permission_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        modelBuilder.Entity<Contract>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("contracts")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.Client, "client");

            entity.HasIndex(e => e.ContractType, "contract_type");

            entity.HasIndex(e => e.ProjectId, "contracts_project_id_fkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AcceptanceDate)
                .HasMaxLength(191)
                .HasColumnName("acceptance_date");
            entity.Property(e => e.AcceptanceEmail).HasColumnName("acceptance_email");
            entity.Property(e => e.AcceptanceFirstName)
                .HasMaxLength(191)
                .HasColumnName("acceptance_first_name");
            entity.Property(e => e.AcceptanceIp)
                .HasMaxLength(191)
                .HasColumnName("acceptance_ip");
            entity.Property(e => e.AcceptanceLastName)
                .HasMaxLength(191)
                .HasColumnName("acceptance_last_name");
            entity.Property(e => e.AddedFrom).HasColumnName("added_from");
            entity.Property(e => e.Client).HasColumnName("client");
            entity.Property(e => e.ContactsSentTo)
                .HasMaxLength(191)
                .HasColumnName("contacts_sent_to");
            entity.Property(e => e.Content)
                .HasMaxLength(191)
                .HasColumnName("content");
            entity.Property(e => e.ContractType).HasColumnName("contract_type");
            entity.Property(e => e.ContractValue).HasColumnName("contract_value");
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(3)")
                .HasColumnType("datetime(3)")
                .HasColumnName("date_created");
            entity.Property(e => e.DateEnd)
                .HasColumnType("datetime(3)")
                .HasColumnName("date_end");
            entity.Property(e => e.DateStart)
                .HasColumnType("datetime(3)")
                .HasColumnName("date_start");
            entity.Property(e => e.Description)
                .HasMaxLength(191)
                .HasColumnName("description");
            entity.Property(e => e.Hash)
                .HasMaxLength(191)
                .HasColumnName("hash");
            entity.Property(e => e.IsExpiryNotified).HasColumnName("is_expiry_notified");
            entity.Property(e => e.LastSentAt)
                .HasColumnType("datetime(3)")
                .HasColumnName("last_sent_at");
            entity.Property(e => e.LastSignReminderAt)
                .HasMaxLength(191)
                .HasColumnName("last_sign_reminder_at");
            entity.Property(e => e.MarkedAsSigned).HasColumnName("marked_as_signed");
            entity.Property(e => e.NotVisibleToClient)
                .HasMaxLength(255)
                .HasColumnName("not_visible_to_client");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.ShortLink)
                .HasMaxLength(191)
                .HasColumnName("short_link");
            entity.Property(e => e.Signature)
                .HasMaxLength(191)
                .HasColumnName("signature");
            entity.Property(e => e.Signed).HasColumnName("signed");
            entity.Property(e => e.Subject)
                .HasMaxLength(191)
                .HasColumnName("subject");
            entity.Property(e => e.Trash).HasColumnName("trash");

            entity.HasOne(d => d.Project).WithMany(p => p.Contracts)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("contracts_project_id_fkey");
        });

        modelBuilder.Entity<ContractComment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("contract_comments")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.ContractId, "contract_comments_contract_id_fkey");

            entity.HasIndex(e => e.StaffId, "contract_comments_staff_id_fkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Content)
                .HasMaxLength(191)
                .HasColumnName("content");
            entity.Property(e => e.ContractId).HasColumnName("contract_id");
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(3)")
                .HasColumnType("datetime(3)")
                .HasColumnName("date_created");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");

            entity.HasOne(d => d.Contract).WithMany(p => p.ContractComments)
                .HasForeignKey(d => d.ContractId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("contract_comments_contract_id_fkey");

            entity.HasOne(d => d.Staff).WithMany(p => p.ContractComments)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("contract_comments_staff_id_fkey");
        });

        modelBuilder.Entity<ContractRenewal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("contract_renewals")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.ContractId, "contract_renewals_contract_id_fkey");

            entity.HasIndex(e => e.RenewedByStaffId, "contract_renewals_renewed_by_staff_id_fkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ContractId).HasColumnName("contract_id");
            entity.Property(e => e.DateRenewed)
                .HasColumnType("datetime(3)")
                .HasColumnName("date_renewed");
            entity.Property(e => e.IsOnOldExpiryNotified).HasColumnName("is_on_old_expiry_notified");
            entity.Property(e => e.NewEndDate)
                .HasColumnType("datetime(3)")
                .HasColumnName("new_end_date");
            entity.Property(e => e.NewStartDate)
                .HasColumnType("datetime(3)")
                .HasColumnName("new_start_date");
            entity.Property(e => e.NewValue).HasColumnName("new_value");
            entity.Property(e => e.OldEndDate)
                .HasColumnType("datetime(3)")
                .HasColumnName("old_end_date");
            entity.Property(e => e.OldStartDate)
                .HasColumnType("datetime(3)")
                .HasColumnName("old_start_date");
            entity.Property(e => e.OldValue).HasColumnName("old_value");
            entity.Property(e => e.RenewedBy)
                .HasMaxLength(191)
                .HasColumnName("renewed_by");
            entity.Property(e => e.RenewedByStaffId).HasColumnName("renewed_by_staff_id");

            entity.HasOne(d => d.Contract).WithMany(p => p.ContractRenewals)
                .HasForeignKey(d => d.ContractId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("contract_renewals_contract_id_fkey");

            entity.HasOne(d => d.RenewedByStaff).WithMany(p => p.ContractRenewals)
                .HasForeignKey(d => d.RenewedByStaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("contract_renewals_renewed_by_staff_id_fkey");
        });

        modelBuilder.Entity<ContractsType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("contracts_types")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(191)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("countries")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CallingCode)
                .HasMaxLength(191)
                .HasColumnName("calling_code");
            entity.Property(e => e.Cctld)
                .HasMaxLength(191)
                .HasColumnName("cctld");
            entity.Property(e => e.Iso2)
                .HasMaxLength(191)
                .HasColumnName("iso2");
            entity.Property(e => e.Iso3)
                .HasMaxLength(191)
                .HasColumnName("iso3");
            entity.Property(e => e.LongName)
                .HasMaxLength(191)
                .HasColumnName("long_name");
            entity.Property(e => e.Numcode)
                .HasMaxLength(191)
                .HasColumnName("numcode");
            entity.Property(e => e.ShortName)
                .HasMaxLength(191)
                .HasColumnName("short_name");
            entity.Property(e => e.UnMember)
                .HasMaxLength(191)
                .HasColumnName("un_member");
        });

        modelBuilder.Entity<Credit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("credits")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.InvoiceId, "credits_invoice_id_fkey");

            entity.HasIndex(e => e.StaffId, "credits_staff_id_fkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.CreditId).HasColumnName("credit_id");
            entity.Property(e => e.Date)
                .HasColumnType("datetime(3)")
                .HasColumnName("date");
            entity.Property(e => e.DateApplied)
                .HasColumnType("datetime(3)")
                .HasColumnName("date_applied");
            entity.Property(e => e.InvoiceId).HasColumnName("invoice_id");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");

            entity.HasOne(d => d.Invoice).WithMany(p => p.Credits)
                .HasForeignKey(d => d.InvoiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("credits_invoice_id_fkey");

            entity.HasOne(d => d.Staff).WithMany(p => p.Credits)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("credits_staff_id_fkey");
        });

        modelBuilder.Entity<CreditNote>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("credit_notes")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.ClientId, "client_id");

            entity.HasIndex(e => e.ProjectId, "credit_notes_project_id_fkey");

            entity.HasIndex(e => e.CurrencyId, "currency");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AddedFrom).HasColumnName("added_from");
            entity.Property(e => e.Adjustment).HasColumnName("adjustment");
            entity.Property(e => e.AdminNote)
                .HasMaxLength(191)
                .HasColumnName("admin_note");
            entity.Property(e => e.BillingCity)
                .HasMaxLength(191)
                .HasColumnName("billing_city");
            entity.Property(e => e.BillingCountry).HasColumnName("billing_country");
            entity.Property(e => e.BillingState)
                .HasMaxLength(191)
                .HasColumnName("billing_state");
            entity.Property(e => e.BillingStreet)
                .HasMaxLength(191)
                .HasColumnName("billing_street");
            entity.Property(e => e.BillingZip)
                .HasMaxLength(191)
                .HasColumnName("billing_zip");
            entity.Property(e => e.ClientId).HasColumnName("client_id");
            entity.Property(e => e.ClientNote)
                .HasMaxLength(191)
                .HasColumnName("client_note");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.Date)
                .HasColumnType("datetime(3)")
                .HasColumnName("date");
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(3)")
                .HasColumnType("datetime(3)")
                .HasColumnName("date_created");
            entity.Property(e => e.DeletedCustomerName)
                .HasMaxLength(191)
                .HasColumnName("deleted_customer_name");
            entity.Property(e => e.DiscountPercent).HasColumnName("discount_percent");
            entity.Property(e => e.DiscountTotal).HasColumnName("discount_total");
            entity.Property(e => e.DiscountType)
                .HasMaxLength(191)
                .HasColumnName("discount_type");
            entity.Property(e => e.IncludeShipping)
                .HasDefaultValueSql("'0'")
                .HasColumnName("include_shipping");
            entity.Property(e => e.Number).HasColumnName("number");
            entity.Property(e => e.NumberFormat).HasColumnName("number_format");
            entity.Property(e => e.Prefix)
                .HasMaxLength(191)
                .HasColumnName("prefix");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.ReferenceNo)
                .HasMaxLength(191)
                .HasColumnName("reference_no");
            entity.Property(e => e.ShippingCity)
                .HasMaxLength(191)
                .HasColumnName("shipping_city");
            entity.Property(e => e.ShippingCountry).HasColumnName("shipping_country");
            entity.Property(e => e.ShippingState)
                .HasMaxLength(191)
                .HasColumnName("shipping_state");
            entity.Property(e => e.ShippingStreet)
                .HasMaxLength(191)
                .HasColumnName("shipping_street");
            entity.Property(e => e.ShippingZip)
                .HasMaxLength(191)
                .HasColumnName("shipping_zip");
            entity.Property(e => e.ShowQuantityAs).HasColumnName("show_quantity_as");
            entity.Property(e => e.ShowShippingOnCreditNote)
                .HasDefaultValueSql("'0'")
                .HasColumnName("show_shipping_on_credit_note");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.Subtotal).HasColumnName("subtotal");
            entity.Property(e => e.Terms)
                .HasMaxLength(191)
                .HasColumnName("terms");
            entity.Property(e => e.Total).HasColumnName("total");
            entity.Property(e => e.TotalTax).HasColumnName("total_tax");

            entity.HasOne(d => d.Client).WithMany(p => p.CreditNotes)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("credit_notes_client_id_fkey");

            entity.HasOne(d => d.Currency).WithMany(p => p.CreditNotes)
                .HasForeignKey(d => d.CurrencyId)
                .HasConstraintName("credit_notes_currency_id_fkey");

            entity.HasOne(d => d.Project).WithMany(p => p.CreditNotes)
                .HasForeignKey(d => d.ProjectId)
                .HasConstraintName("credit_notes_project_id_fkey");
        });

        modelBuilder.Entity<CreditNoteRefund>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("credit_note_refunds")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.CreditNoteId, "creditnote_refunds_credit_note_id_fkey");

            entity.HasIndex(e => e.StaffId, "creditnote_refunds_staff_id_fkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreditNoteId).HasColumnName("credit_note_id");
            entity.Property(e => e.Note)
                .HasMaxLength(191)
                .HasColumnName("note");
            entity.Property(e => e.PaymentMode)
                .HasMaxLength(191)
                .HasColumnName("payment_mode");
            entity.Property(e => e.RefundedOn)
                .HasMaxLength(191)
                .HasColumnName("refunded_on");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");

            entity.HasOne(d => d.CreditNote).WithMany(p => p.CreditNoteRefunds)
                .HasForeignKey(d => d.CreditNoteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("creditnote_refunds_credit_note_id_fkey");

            entity.HasOne(d => d.Staff).WithMany(p => p.CreditNoteRefunds)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("creditnote_refunds_staff_id_fkey");
        });

        modelBuilder.Entity<Currency>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("currencies")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DecimalSeparator)
                .HasMaxLength(191)
                .HasColumnName("decimal_separator");
            entity.Property(e => e.IsDefault).HasColumnName("is_default");
            entity.Property(e => e.Name)
                .HasMaxLength(191)
                .HasColumnName("name");
            entity.Property(e => e.Placement)
                .HasMaxLength(191)
                .HasColumnName("placement");
            entity.Property(e => e.Symbol)
                .HasMaxLength(191)
                .HasColumnName("symbol");
            entity.Property(e => e.ThousandSeparator)
                .HasMaxLength(191)
                .HasColumnName("thousand_separator");
        });

        modelBuilder.Entity<CustomField>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("custom_fields")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.BsColumn).HasColumnName("bs_column");
            entity.Property(e => e.DefaultValue)
                .HasMaxLength(191)
                .HasColumnName("default_value");
            entity.Property(e => e.DisalowClientToEdit).HasColumnName("disalow_client_to_edit");
            entity.Property(e => e.DisplayInline).HasColumnName("display_inline");
            entity.Property(e => e.FieldOrder).HasColumnName("field_order");
            entity.Property(e => e.FieldTo)
                .HasMaxLength(191)
                .HasColumnName("field_to");
            entity.Property(e => e.Name)
                .HasMaxLength(191)
                .HasColumnName("name");
            entity.Property(e => e.OnlyAdmin).HasColumnName("only_admin");
            entity.Property(e => e.Options)
                .HasMaxLength(191)
                .HasColumnName("options");
            entity.Property(e => e.Required).HasColumnName("required");
            entity.Property(e => e.ShowOnClientPortal).HasColumnName("show_on_client_portal");
            entity.Property(e => e.ShowOnPdf).HasColumnName("show_on_pdf");
            entity.Property(e => e.ShowOnTable).HasColumnName("show_on_table");
            entity.Property(e => e.ShowOnTicketForm).HasColumnName("show_on_ticket_form");
            entity.Property(e => e.Slug)
                .HasMaxLength(191)
                .HasColumnName("slug");
            entity.Property(e => e.Type)
                .HasMaxLength(191)
                .HasColumnName("type");
        });

        modelBuilder.Entity<CustomFieldsValue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("custom_fields_values")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.FieldId, "field_id");

            entity.HasIndex(e => e.FieldTo, "field_to");

            entity.HasIndex(e => e.RelId, "rel_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FieldId).HasColumnName("field_id");
            entity.Property(e => e.FieldTo)
                .HasMaxLength(191)
                .HasColumnName("field_to");
            entity.Property(e => e.RelId).HasColumnName("rel_id");
            entity.Property(e => e.Value)
                .HasMaxLength(191)
                .HasColumnName("value");

            entity.HasOne(d => d.Field).WithMany(p => p.CustomFieldsValues)
                .HasForeignKey(d => d.FieldId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("custom_fields_values_field_id_fkey");
        });

        modelBuilder.Entity<CustomerAdmin>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("customer_admins")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.CustomerId, "customer_id");

            entity.HasIndex(e => e.StaffId, "staff_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.DateAssigned)
                .HasMaxLength(191)
                .HasColumnName("date_assigned");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");

            entity.HasOne(d => d.Customer).WithMany(p => p.CustomerAdmins)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("customer_admins_customer_id_fkey");

            entity.HasOne(d => d.Staff).WithMany(p => p.CustomerAdmins)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("customer_admins_staff_id_fkey");
        });

        modelBuilder.Entity<CustomerGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("customer_groups")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.CustomerId, "customer_groups_customer_id_fkey");

            entity.HasIndex(e => e.GroupId, "groupid");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.GroupId).HasColumnName("group_id");

            entity.HasOne(d => d.Customer).WithMany(p => p.CustomerGroups)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("customer_groups_customer_id_fkey");

            entity.HasOne(d => d.Group).WithMany(p => p.CustomerGroups)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("customer_groups_group_id_fkey");
        });

        modelBuilder.Entity<CustomersGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("customers_groups")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.Name, "name");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(191)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("departments")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CalendarId)
                .HasMaxLength(191)
                .HasColumnName("calendar_id");
            entity.Property(e => e.DeleteAfterImport).HasColumnName("delete_after_import");
            entity.Property(e => e.Email)
                .HasMaxLength(191)
                .HasColumnName("email");
            entity.Property(e => e.EmailFromHeader).HasColumnName("email_from_header");
            entity.Property(e => e.Encryption)
                .HasMaxLength(191)
                .HasColumnName("encryption");
            entity.Property(e => e.Folder)
                .HasMaxLength(191)
                .HasColumnName("folder");
            entity.Property(e => e.HideFromClient).HasColumnName("hide_from_client");
            entity.Property(e => e.Host)
                .HasMaxLength(191)
                .HasColumnName("host");
            entity.Property(e => e.ImapUsername)
                .HasMaxLength(191)
                .HasColumnName("imap_username");
            entity.Property(e => e.Name)
                .HasMaxLength(191)
                .HasColumnName("name");
            entity.Property(e => e.Password)
                .HasMaxLength(191)
                .HasColumnName("password");
        });

        modelBuilder.Entity<DismissedAnnouncement>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("dismissed_announcements")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.AnnouncementId, "announcementid");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AnnouncementId).HasColumnName("announcement_id");
            entity.Property(e => e.DateRead)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(3)")
                .HasColumnType("datetime(3)")
                .HasColumnName("date_read");
            entity.Property(e => e.IsStaff).HasColumnName("is_staff");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Announcement).WithMany(p => p.DismissedAnnouncements)
                .HasForeignKey(d => d.AnnouncementId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("dismissed_announcements_announcement_id_fkey");
        });

        modelBuilder.Entity<EmailTemplate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("email_templates")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.FromEmail)
                .HasMaxLength(191)
                .HasColumnName("from_email");
            entity.Property(e => e.FromName)
                .HasMaxLength(191)
                .HasColumnName("from_name");
            entity.Property(e => e.Language)
                .HasMaxLength(191)
                .HasColumnName("language");
            entity.Property(e => e.Message)
                .HasMaxLength(191)
                .HasColumnName("message");
            entity.Property(e => e.Name)
                .HasMaxLength(191)
                .HasColumnName("name");
            entity.Property(e => e.Order).HasColumnName("order");
            entity.Property(e => e.PlainText).HasColumnName("plain_text");
            entity.Property(e => e.Slug)
                .HasMaxLength(191)
                .HasColumnName("slug");
            entity.Property(e => e.Subject)
                .HasMaxLength(191)
                .HasColumnName("subject");
            entity.Property(e => e.Type)
                .HasMaxLength(191)
                .HasColumnName("type");
        });

        modelBuilder.Entity<Estimate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("estimates")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.ClientId, "estimates_client_id_fkey");

            entity.HasIndex(e => e.CurrencyId, "estimates_currency_id_fkey");

            entity.HasIndex(e => e.InvoiceId, "estimates_invoice_id_fkey");

            entity.HasIndex(e => e.ProjectId, "estimates_project_id_fkey");

            entity.HasIndex(e => e.SaleAgent, "sale_agent");

            entity.HasIndex(e => e.Status, "status");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AcceptanceDate)
                .HasMaxLength(191)
                .HasColumnName("acceptance_date");
            entity.Property(e => e.AcceptanceEmail)
                .HasMaxLength(191)
                .HasColumnName("acceptance_email");
            entity.Property(e => e.AcceptanceFirstName)
                .HasMaxLength(191)
                .HasColumnName("acceptance_first_name");
            entity.Property(e => e.AcceptanceIp)
                .HasMaxLength(191)
                .HasColumnName("acceptance_ip");
            entity.Property(e => e.AcceptanceLastName)
                .HasMaxLength(191)
                .HasColumnName("acceptance_last_name");
            entity.Property(e => e.AddedFrom).HasColumnName("added_from");
            entity.Property(e => e.Adjustment).HasColumnName("adjustment");
            entity.Property(e => e.AdminNote)
                .HasMaxLength(191)
                .HasColumnName("admin_note");
            entity.Property(e => e.BillingCity)
                .HasMaxLength(191)
                .HasColumnName("billing_city");
            entity.Property(e => e.BillingCountry).HasColumnName("billing_country");
            entity.Property(e => e.BillingState)
                .HasMaxLength(191)
                .HasColumnName("billing_state");
            entity.Property(e => e.BillingStreet)
                .HasMaxLength(191)
                .HasColumnName("billing_street");
            entity.Property(e => e.BillingZip)
                .HasMaxLength(191)
                .HasColumnName("billing_zip");
            entity.Property(e => e.ClientId).HasColumnName("client_id");
            entity.Property(e => e.ClientNote)
                .HasMaxLength(191)
                .HasColumnName("client_note");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasColumnName("date");
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("date_created");
            entity.Property(e => e.DateSend)
                .HasMaxLength(191)
                .HasColumnName("date_send");
            entity.Property(e => e.DeletedCustomerName)
                .HasMaxLength(191)
                .HasColumnName("deleted_customer_name");
            entity.Property(e => e.DiscountPercent).HasColumnName("discount_percent");
            entity.Property(e => e.DiscountTotal).HasColumnName("discount_total");
            entity.Property(e => e.DiscountType)
                .HasMaxLength(191)
                .HasColumnName("discount_type");
            entity.Property(e => e.ExpiryDate)
                .HasColumnType("datetime")
                .HasColumnName("expiry_date");
            entity.Property(e => e.Hash)
                .HasMaxLength(191)
                .HasColumnName("hash");
            entity.Property(e => e.IncludeShipping).HasColumnName("include_shipping");
            entity.Property(e => e.InvoiceId).HasColumnName("invoice_id");
            entity.Property(e => e.InvoicedDate)
                .HasMaxLength(191)
                .HasColumnName("invoiced_date");
            entity.Property(e => e.IsExpiryNotified).HasColumnName("is_expiry_notified");
            entity.Property(e => e.Number).HasColumnName("number");
            entity.Property(e => e.NumberFormat).HasColumnName("number_format");
            entity.Property(e => e.PipelineOrder).HasColumnName("pipeline_order");
            entity.Property(e => e.Prefix)
                .HasMaxLength(191)
                .HasColumnName("prefix");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.ReferenceNo)
                .HasMaxLength(191)
                .HasColumnName("reference_no");
            entity.Property(e => e.SaleAgent).HasColumnName("sale_agent");
            entity.Property(e => e.Sent).HasColumnName("sent");
            entity.Property(e => e.ShippingCity)
                .HasMaxLength(191)
                .HasColumnName("shipping_city");
            entity.Property(e => e.ShippingCountry).HasColumnName("shipping_country");
            entity.Property(e => e.ShippingState)
                .HasMaxLength(191)
                .HasColumnName("shipping_state");
            entity.Property(e => e.ShippingStreet)
                .HasMaxLength(191)
                .HasColumnName("shipping_street");
            entity.Property(e => e.ShippingZip)
                .HasMaxLength(191)
                .HasColumnName("shipping_zip");
            entity.Property(e => e.ShortLink)
                .HasMaxLength(191)
                .HasColumnName("short_link");
            entity.Property(e => e.ShowQuantityAs).HasColumnName("show_quantity_as");
            entity.Property(e => e.ShowShippingOnEstimate).HasColumnName("show_shipping_on_estimate");
            entity.Property(e => e.Signature)
                .HasMaxLength(191)
                .HasColumnName("signature");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.Subtotal).HasColumnName("subtotal");
            entity.Property(e => e.Terms)
                .HasMaxLength(191)
                .HasColumnName("terms");
            entity.Property(e => e.Total).HasColumnName("total");
            entity.Property(e => e.TotalTax).HasColumnName("total_tax");

            entity.HasOne(d => d.Client).WithMany(p => p.Estimates)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("estimates_client_id_fkey");

            entity.HasOne(d => d.Currency).WithMany(p => p.Estimates)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("estimates_currency_id_fkey");

            entity.HasOne(d => d.Invoice).WithMany(p => p.Estimates)
                .HasForeignKey(d => d.InvoiceId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("estimates_invoice_id_fkey");

            entity.HasOne(d => d.Project).WithMany(p => p.Estimates)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("estimates_project_id_fkey");

            entity.HasOne(d => d.SaleAgentNavigation).WithMany(p => p.Estimates)
                .HasForeignKey(d => d.SaleAgent)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("estimates_sale_agent_fkey");
        });

        modelBuilder.Entity<EstimateRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("estimate_requests")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.FromFormId, "estimate_requests_from_form_id_fkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Assigned).HasColumnName("assigned");
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(3)")
                .HasColumnType("datetime(3)")
                .HasColumnName("date_created");
            entity.Property(e => e.DateEstimated)
                .HasMaxLength(191)
                .HasColumnName("date_estimated");
            entity.Property(e => e.DefaultLanguage).HasColumnName("default_language");
            entity.Property(e => e.Email)
                .HasMaxLength(191)
                .HasColumnName("email");
            entity.Property(e => e.FromFormId).HasColumnName("from_form_id");
            entity.Property(e => e.LastStatusChange)
                .HasMaxLength(191)
                .HasColumnName("last_status_change");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.Submission)
                .HasMaxLength(191)
                .HasColumnName("submission");

            entity.HasOne(d => d.FromForm).WithMany(p => p.EstimateRequests)
                .HasForeignKey(d => d.FromFormId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("estimate_requests_from_form_id_fkey");
        });

        modelBuilder.Entity<EstimateRequestForm>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("estimate_request_forms")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(3)")
                .HasColumnType("datetime(3)")
                .HasColumnName("date_created");
            entity.Property(e => e.FormData)
                .HasMaxLength(191)
                .HasColumnName("form_data");
            entity.Property(e => e.FormKey)
                .HasMaxLength(191)
                .HasColumnName("form_key");
            entity.Property(e => e.Language)
                .HasMaxLength(191)
                .HasColumnName("language");
            entity.Property(e => e.Name)
                .HasMaxLength(191)
                .HasColumnName("name");
            entity.Property(e => e.NotifyIds)
                .HasMaxLength(191)
                .HasColumnName("notify_ids");
            entity.Property(e => e.NotifyRequestSubmitted).HasColumnName("notify_request_submitted");
            entity.Property(e => e.NotifyType)
                .HasMaxLength(191)
                .HasColumnName("notify_type");
            entity.Property(e => e.Recaptcha).HasColumnName("recaptcha");
            entity.Property(e => e.Responsible).HasColumnName("responsible");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.SubmitAction).HasColumnName("submit_action");
            entity.Property(e => e.SubmitBtnBgColor)
                .HasMaxLength(191)
                .HasColumnName("submit_btn_bg_color");
            entity.Property(e => e.SubmitBtnName)
                .HasMaxLength(191)
                .HasColumnName("submit_btn_name");
            entity.Property(e => e.SubmitBtnTextColor)
                .HasMaxLength(191)
                .HasColumnName("submit_btn_text_color");
            entity.Property(e => e.SubmitRedirectUrl)
                .HasMaxLength(191)
                .HasColumnName("submit_redirect_url");
            entity.Property(e => e.SuccessSubmitMsg)
                .HasMaxLength(191)
                .HasColumnName("success_submit_msg");
            entity.Property(e => e.Type)
                .HasMaxLength(191)
                .HasColumnName("type");
        });

        modelBuilder.Entity<EstimateRequestStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("estimate_request_status")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Color)
                .HasMaxLength(191)
                .HasColumnName("color");
            entity.Property(e => e.Flag)
                .HasMaxLength(191)
                .HasColumnName("flag");
            entity.Property(e => e.Name)
                .HasMaxLength(191)
                .HasColumnName("name");
            entity.Property(e => e.StatusOrder).HasColumnName("status_order");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("events")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.UserId, "events_user_id_fkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Color)
                .HasMaxLength(191)
                .HasColumnName("color");
            entity.Property(e => e.Description)
                .HasMaxLength(191)
                .HasColumnName("description");
            entity.Property(e => e.End)
                .HasMaxLength(191)
                .HasColumnName("end");
            entity.Property(e => e.IsStartNotified).HasColumnName("is_start_notified");
            entity.Property(e => e.Public).HasColumnName("public");
            entity.Property(e => e.ReminderBefore).HasColumnName("reminder_before");
            entity.Property(e => e.ReminderBeforeType)
                .HasMaxLength(191)
                .HasColumnName("reminder_before_type");
            entity.Property(e => e.Start)
                .HasMaxLength(191)
                .HasColumnName("start");
            entity.Property(e => e.Title)
                .HasMaxLength(191)
                .HasColumnName("title");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Events)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("events_user_id_fkey");
        });

        modelBuilder.Entity<Expense>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("expenses")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.CategoryId, "category");

            entity.HasIndex(e => e.ClientId, "expenses_client_id_fkey");

            entity.HasIndex(e => e.InvoiceId, "expenses_invoice_id_fkey");

            entity.HasIndex(e => e.ProjectId, "expenses_project_id_fkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AddedFrom).HasColumnName("added_from");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.Billable)
                .HasDefaultValueSql("'0'")
                .HasColumnName("billable");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.ClientId).HasColumnName("client_id");
            entity.Property(e => e.CreateInvoiceBillable)
                .HasDefaultValueSql("'0'")
                .HasColumnName("create_invoice_billable");
            entity.Property(e => e.Currency).HasColumnName("currency");
            entity.Property(e => e.CustomRecurring).HasColumnName("custom_recurring");
            entity.Property(e => e.Cycles).HasColumnName("cycles");
            entity.Property(e => e.Date)
                .HasMaxLength(191)
                .HasColumnName("date");
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(3)")
                .HasColumnType("datetime(3)")
                .HasColumnName("date_created");
            entity.Property(e => e.ExpenseName)
                .HasMaxLength(191)
                .HasColumnName("expense_name");
            entity.Property(e => e.InvoiceId).HasColumnName("invoice_id");
            entity.Property(e => e.LastRecurringDate)
                .HasMaxLength(191)
                .HasColumnName("last_recurring_date");
            entity.Property(e => e.Note)
                .HasMaxLength(191)
                .HasColumnName("note");
            entity.Property(e => e.PaymentMode)
                .HasMaxLength(191)
                .HasColumnName("payment_mode");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.Recurring).HasColumnName("recurring");
            entity.Property(e => e.RecurringFrom).HasColumnName("recurring_from");
            entity.Property(e => e.RecurringType)
                .HasMaxLength(191)
                .HasColumnName("recurring_type");
            entity.Property(e => e.ReferenceNo)
                .HasMaxLength(191)
                .HasColumnName("reference_no");
            entity.Property(e => e.RepeatEvery)
                .HasMaxLength(191)
                .HasDefaultValueSql("'1'")
                .HasColumnName("repeat_every");
            entity.Property(e => e.SendInvoiceToCustomer).HasColumnName("send_invoice_to_customer");
            entity.Property(e => e.Tax).HasColumnName("tax");
            entity.Property(e => e.Tax2).HasColumnName("tax2");
            entity.Property(e => e.TotalCycles).HasColumnName("total_cycles");

            entity.HasOne(d => d.Category).WithMany(p => p.Expenses)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("expenses_category_id_fkey");

            entity.HasOne(d => d.Client).WithMany(p => p.Expenses)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("expenses_client_id_fkey");

            entity.HasOne(d => d.Invoice).WithMany(p => p.Expenses)
                .HasForeignKey(d => d.InvoiceId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("expenses_invoice_id_fkey");

            entity.HasOne(d => d.Project).WithMany(p => p.Expenses)
                .HasForeignKey(d => d.ProjectId)
                .HasConstraintName("expenses_project_id_fkey");
        });

        modelBuilder.Entity<ExpensesCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("expenses_categories")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(191)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(191)
                .HasColumnName("name");
        });

        modelBuilder.Entity<File>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.Uuid })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity
                .ToTable("files")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.ContactId, "files_contact_id_fkey");

            entity.HasIndex(e => e.Id, "files_id_key").IsUnique();

            entity.HasIndex(e => e.StaffId, "files_staff_id_fkey");

            entity.HasIndex(e => e.RelType, "rel_type");

            entity.HasIndex(e => e.RelId, "relid");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");
            entity.Property(e => e.Uuid)
                .HasMaxLength(36)
                .HasColumnName("uuid");
            entity.Property(e => e.AttachmentKey)
                .HasMaxLength(191)
                .HasColumnName("attachment_key");
            entity.Property(e => e.ContactId).HasColumnName("contact_id");
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(3)")
                .HasColumnType("datetime(3)")
                .HasColumnName("date_created");
            entity.Property(e => e.External)
                .HasMaxLength(191)
                .HasColumnName("external");
            entity.Property(e => e.ExternalLink)
                .HasMaxLength(191)
                .HasColumnName("external_link");
            entity.Property(e => e.FileName)
                .HasMaxLength(191)
                .HasColumnName("file_name");
            entity.Property(e => e.FileType)
                .HasMaxLength(191)
                .HasColumnName("file_type");
            entity.Property(e => e.RelId).HasColumnName("rel_id");
            entity.Property(e => e.RelType)
                .HasMaxLength(191)
                .HasColumnName("rel_type");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");
            entity.Property(e => e.TaskCommentId).HasColumnName("task_comment_id");
            entity.Property(e => e.ThumbnailLink)
                .HasMaxLength(191)
                .HasColumnName("thumbnail_link");
            entity.Property(e => e.VisibleToCustomer).HasColumnName("visible_to_customer");

            entity.HasOne(d => d.Contact).WithMany(p => p.Files)
                .HasForeignKey(d => d.ContactId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("files_contact_id_fkey");

            entity.HasOne(d => d.Staff).WithMany(p => p.Files)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("files_staff_id_fkey");
        });

        modelBuilder.Entity<FormQuestion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("form_questions")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Question)
                .HasMaxLength(191)
                .HasColumnName("question");
            entity.Property(e => e.QuestionOrder).HasColumnName("question_order");
            entity.Property(e => e.RelId).HasColumnName("rel_id");
            entity.Property(e => e.RelType)
                .HasMaxLength(191)
                .HasColumnName("rel_type");
            entity.Property(e => e.Required).HasColumnName("required");
        });

        modelBuilder.Entity<FormQuestionBox>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("form_question_box")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.QuestionId).HasColumnName("question_id");
            entity.Property(e => e.Type)
                .HasMaxLength(191)
                .HasColumnName("type");
        });

        modelBuilder.Entity<FormQuestionBoxDescription>(entity =>
        {
            entity.HasKey(e => e.Questionboxdescriptionid).HasName("PRIMARY");

            entity
                .ToTable("form_question_box_description")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Questionboxdescriptionid).HasColumnName("questionboxdescriptionid");
            entity.Property(e => e.BoxId)
                .HasMaxLength(191)
                .HasColumnName("box_id");
            entity.Property(e => e.Description)
                .HasMaxLength(191)
                .HasColumnName("description");
            entity.Property(e => e.QuestionId).HasColumnName("question_id");
        });

        modelBuilder.Entity<FormResult>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("form_results")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Answer)
                .HasMaxLength(191)
                .HasColumnName("answer");
            entity.Property(e => e.BoxId).HasColumnName("box_id");
            entity.Property(e => e.Boxdescriptionid).HasColumnName("boxdescriptionid");
            entity.Property(e => e.QuestionId).HasColumnName("question_id");
            entity.Property(e => e.RelId).HasColumnName("rel_id");
            entity.Property(e => e.RelType)
                .HasMaxLength(191)
                .HasColumnName("rel_type");
            entity.Property(e => e.ResultsetId).HasColumnName("resultset_id");
        });

        modelBuilder.Entity<GdprRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("gdpr_requests")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.ClientId, "gdpr_requests_client_id_fkey");

            entity.HasIndex(e => e.ContactId, "gdpr_requests_contact_id_fkey");

            entity.HasIndex(e => e.LeadId, "gdpr_requests_lead_id_fkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClientId).HasColumnName("client_id");
            entity.Property(e => e.ContactId).HasColumnName("contact_id");
            entity.Property(e => e.Description)
                .HasMaxLength(191)
                .HasColumnName("description");
            entity.Property(e => e.LeadId).HasColumnName("lead_id");
            entity.Property(e => e.RequestDate)
                .HasMaxLength(191)
                .HasColumnName("request_date");
            entity.Property(e => e.RequestFrom)
                .HasMaxLength(191)
                .HasColumnName("request_from");
            entity.Property(e => e.RequestType)
                .HasMaxLength(191)
                .HasColumnName("request_type");
            entity.Property(e => e.Status)
                .HasMaxLength(191)
                .HasColumnName("status");

            entity.HasOne(d => d.Client).WithMany(p => p.GdprRequests)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("gdpr_requests_client_id_fkey");

            entity.HasOne(d => d.Contact).WithMany(p => p.GdprRequests)
                .HasForeignKey(d => d.ContactId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("gdpr_requests_contact_id_fkey");

            entity.HasOne(d => d.Lead).WithMany(p => p.GdprRequests)
                .HasForeignKey(d => d.LeadId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("gdpr_requests_lead_id_fkey");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("invoices")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.ClientId, "invoices_client_id_fkey");

            entity.HasIndex(e => e.CurrencyId, "invoices_currency_id_fkey");

            entity.HasIndex(e => e.ProjectId, "invoices_project_id_fkey");

            entity.HasIndex(e => e.SaleAgent, "invoices_sale_agent_fkey");

            entity.HasIndex(e => e.SubscriptionId, "invoices_subscription_id_fkey");

            entity.HasIndex(e => e.Total, "total");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AddedFrom).HasColumnName("added_from");
            entity.Property(e => e.Adjustment).HasColumnName("adjustment");
            entity.Property(e => e.AdminNote)
                .HasMaxLength(191)
                .HasColumnName("admin_note");
            entity.Property(e => e.AllowedPaymentModes)
                .HasMaxLength(191)
                .HasColumnName("allowed_payment_modes");
            entity.Property(e => e.BillingCity)
                .HasMaxLength(191)
                .HasColumnName("billing_city");
            entity.Property(e => e.BillingCountry).HasColumnName("billing_country");
            entity.Property(e => e.BillingState)
                .HasMaxLength(191)
                .HasColumnName("billing_state");
            entity.Property(e => e.BillingStreet)
                .HasMaxLength(191)
                .HasColumnName("billing_street");
            entity.Property(e => e.BillingZip)
                .HasMaxLength(191)
                .HasColumnName("billing_zip");
            entity.Property(e => e.CancelOverdueReminders).HasColumnName("cancel_overdue_reminders");
            entity.Property(e => e.ClientId).HasColumnName("client_id");
            entity.Property(e => e.ClientNote)
                .HasMaxLength(191)
                .HasColumnName("client_note");
            entity.Property(e => e.CurrencyId).HasColumnName("currency_id");
            entity.Property(e => e.CustomRecurring).HasColumnName("custom_recurring");
            entity.Property(e => e.Cycles).HasColumnName("cycles");
            entity.Property(e => e.Date)
                .HasMaxLength(191)
                .HasColumnName("date");
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(3)")
                .HasColumnType("datetime(3)")
                .HasColumnName("date_created");
            entity.Property(e => e.DateSend)
                .HasColumnType("datetime")
                .HasColumnName("date_send");
            entity.Property(e => e.DeletedCustomerName)
                .HasMaxLength(191)
                .HasColumnName("deleted_customer_name");
            entity.Property(e => e.DiscountPercent).HasColumnName("discount_percent");
            entity.Property(e => e.DiscountTotal).HasColumnName("discount_total");
            entity.Property(e => e.DiscountType)
                .HasMaxLength(191)
                .HasColumnName("discount_type");
            entity.Property(e => e.DueDate)
                .HasMaxLength(191)
                .HasColumnName("due_date");
            entity.Property(e => e.Hash)
                .HasMaxLength(191)
                .HasColumnName("hash");
            entity.Property(e => e.IncludeShipping).HasColumnName("include_shipping");
            entity.Property(e => e.IsRecurringFrom).HasColumnName("is_recurring_from");
            entity.Property(e => e.LastDueReminder)
                .HasMaxLength(191)
                .HasColumnName("last_due_reminder");
            entity.Property(e => e.LastOverdueReminder)
                .HasMaxLength(191)
                .HasColumnName("last_overdue_reminder");
            entity.Property(e => e.LastRecurringDate)
                .HasMaxLength(191)
                .HasColumnName("last_recurring_date");
            entity.Property(e => e.Number).HasColumnName("number");
            entity.Property(e => e.NumberFormat).HasColumnName("number_format");
            entity.Property(e => e.Prefix)
                .HasMaxLength(191)
                .HasColumnName("prefix");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.Recurring)
                .HasMaxLength(255)
                .HasColumnName("recurring");
            entity.Property(e => e.RecurringType)
                .HasMaxLength(191)
                .HasColumnName("recurring_type");
            entity.Property(e => e.SaleAgent).HasColumnName("sale_agent");
            entity.Property(e => e.Sent).HasColumnName("sent");
            entity.Property(e => e.ShippingCity)
                .HasMaxLength(191)
                .HasColumnName("shipping_city");
            entity.Property(e => e.ShippingCountry).HasColumnName("shipping_country");
            entity.Property(e => e.ShippingState)
                .HasMaxLength(191)
                .HasColumnName("shipping_state");
            entity.Property(e => e.ShippingStreet)
                .HasMaxLength(191)
                .HasColumnName("shipping_street");
            entity.Property(e => e.ShippingZip)
                .HasMaxLength(191)
                .HasColumnName("shipping_zip");
            entity.Property(e => e.ShortLink)
                .HasMaxLength(191)
                .HasColumnName("short_link");
            entity.Property(e => e.ShowQuantityAs).HasColumnName("show_quantity_as");
            entity.Property(e => e.ShowShippingOnInvoice).HasColumnName("show_shipping_on_invoice");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.SubscriptionId).HasColumnName("subscription_id");
            entity.Property(e => e.Subtotal).HasColumnName("subtotal");
            entity.Property(e => e.Terms)
                .HasMaxLength(191)
                .HasColumnName("terms");
            entity.Property(e => e.Token)
                .HasMaxLength(191)
                .HasColumnName("token");
            entity.Property(e => e.Total).HasColumnName("total");
            entity.Property(e => e.TotalCycles).HasColumnName("total_cycles");
            entity.Property(e => e.TotalTax).HasColumnName("total_tax");

            entity.HasOne(d => d.Client).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("invoices_client_id_fkey");

            entity.HasOne(d => d.Currency).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("invoices_currency_id_fkey");

            entity.HasOne(d => d.Project).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("invoices_project_id_fkey");

            entity.HasOne(d => d.SaleAgentNavigation).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.SaleAgent)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("invoices_sale_agent_fkey");

            entity.HasOne(d => d.Subscription).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.SubscriptionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("invoices_subscription_id_fkey");
        });

        modelBuilder.Entity<InvoicePaymentRecord>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("invoice_payment_records")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.InvoiceId, "invoiceid");

            entity.HasIndex(e => e.PaymentMethod, "paymentmethod");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.Date)
                .HasMaxLength(191)
                .HasColumnName("date");
            entity.Property(e => e.DateRecorded)
                .HasMaxLength(191)
                .HasColumnName("date_recorded");
            entity.Property(e => e.InvoiceId).HasColumnName("invoice_id");
            entity.Property(e => e.Name)
                .HasMaxLength(191)
                .HasColumnName("name");
            entity.Property(e => e.Note)
                .HasMaxLength(191)
                .HasColumnName("note");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(191)
                .HasColumnName("payment_method");
            entity.Property(e => e.PaymentMode)
                .HasMaxLength(191)
                .HasColumnName("payment_mode");
            entity.Property(e => e.TransactionId)
                .HasMaxLength(191)
                .HasColumnName("transaction_id");

            entity.HasOne(d => d.Invoice).WithMany(p => p.InvoicePaymentRecords)
                .HasForeignKey(d => d.InvoiceId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("invoice_payment_records_invoice_id_fkey");
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("items")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.GroupId, "group_id");

            entity.HasIndex(e => e.Tax, "tax");

            entity.HasIndex(e => e.Tax2, "tax2");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(191)
                .HasColumnName("description");
            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.LongDescription)
                .HasMaxLength(191)
                .HasColumnName("long_description");
            entity.Property(e => e.Rate).HasColumnName("rate");
            entity.Property(e => e.Tax).HasColumnName("tax");
            entity.Property(e => e.Tax2).HasColumnName("tax2");
            entity.Property(e => e.Unit)
                .HasMaxLength(191)
                .HasColumnName("unit");

            entity.HasOne(d => d.Group).WithMany(p => p.Items)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("items_group_id_fkey");
        });

        modelBuilder.Entity<ItemTax>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("item_tax")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.ItemId, "itemid");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ItemId).HasColumnName("item_id");
            entity.Property(e => e.RelId).HasColumnName("rel_id");
            entity.Property(e => e.RelType)
                .HasMaxLength(191)
                .HasColumnName("rel_type");
            entity.Property(e => e.TaxName)
                .HasMaxLength(191)
                .HasColumnName("tax_name");
            entity.Property(e => e.TaxRate)
                .HasPrecision(10)
                .HasColumnName("tax_rate");

            entity.HasOne(d => d.Item).WithMany(p => p.ItemTaxes)
                .HasForeignKey(d => d.ItemId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("item_tax_item_id_fkey");
        });

        modelBuilder.Entity<Itemable>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("itemable")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.Qty, "qty");

            entity.HasIndex(e => e.Rate, "rate");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(191)
                .HasColumnName("description");
            entity.Property(e => e.ItemOrder).HasColumnName("item_order");
            entity.Property(e => e.LongDescription)
                .HasMaxLength(191)
                .HasColumnName("long_description");
            entity.Property(e => e.Qty).HasColumnName("qty");
            entity.Property(e => e.Rate).HasColumnName("rate");
            entity.Property(e => e.RelId).HasColumnName("rel_id");
            entity.Property(e => e.RelType)
                .HasMaxLength(191)
                .HasColumnName("rel_type");
            entity.Property(e => e.Unit)
                .HasMaxLength(191)
                .HasColumnName("unit");
        });

        modelBuilder.Entity<ItemsGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("items_groups")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(191)
                .HasColumnName("name");
        });

        modelBuilder.Entity<KnowedgeBaseArticleFeedback>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("knowedge_base_article_feedback")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.ArticleId, "knowedge_base_article_feedback_article_id_fkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Answer).HasColumnName("answer");
            entity.Property(e => e.ArticleId).HasColumnName("article_id");
            entity.Property(e => e.Date)
                .HasColumnType("datetime(3)")
                .HasColumnName("date");
            entity.Property(e => e.Ip)
                .HasMaxLength(191)
                .HasColumnName("ip");

            entity.HasOne(d => d.Article).WithMany(p => p.KnowedgeBaseArticleFeedbacks)
                .HasForeignKey(d => d.ArticleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("knowedge_base_article_feedback_article_id_fkey");
        });

        modelBuilder.Entity<KnowledgeBase>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("knowledge_base")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.ArticleGroupId, "knowledge_base_article_group_id_fkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.ArticleGroupId).HasColumnName("article_group_id");
            entity.Property(e => e.ArticleOrder).HasColumnName("article_order");
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(3)")
                .HasColumnType("datetime(3)")
                .HasColumnName("date_created");
            entity.Property(e => e.Description)
                .HasMaxLength(191)
                .HasColumnName("description");
            entity.Property(e => e.Slug)
                .HasMaxLength(191)
                .HasColumnName("slug");
            entity.Property(e => e.StaffArticle).HasColumnName("staff_article");
            entity.Property(e => e.Subject)
                .HasMaxLength(191)
                .HasColumnName("subject");

            entity.HasOne(d => d.ArticleGroup).WithMany(p => p.KnowledgeBases)
                .HasForeignKey(d => d.ArticleGroupId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("knowledge_base_article_group_id_fkey");
        });

        modelBuilder.Entity<KnowledgeBaseGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("knowledge_base_groups")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.Color)
                .HasMaxLength(191)
                .HasColumnName("color");
            entity.Property(e => e.Description)
                .HasMaxLength(191)
                .HasColumnName("description");
            entity.Property(e => e.GroupOrder).HasColumnName("group_order");
            entity.Property(e => e.GroupSlug)
                .HasMaxLength(191)
                .HasColumnName("group_slug");
            entity.Property(e => e.Name)
                .HasMaxLength(191)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Lead>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("leads")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.Assigned, "assigned");

            entity.HasIndex(e => e.FromFormId, "from_form_id");

            entity.HasIndex(e => e.LastContact, "lastcontact");

            entity.HasIndex(e => e.LeadOrder, "leadorder");

            entity.HasIndex(e => e.StatusId, "leads_status_id_fkey");

            entity.HasIndex(e => e.SourceId, "source");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AddedFrom).HasColumnName("added_from");
            entity.Property(e => e.Address)
                .HasMaxLength(191)
                .HasColumnName("address");
            entity.Property(e => e.Assigned).HasColumnName("assigned");
            entity.Property(e => e.City)
                .HasMaxLength(191)
                .HasColumnName("city");
            entity.Property(e => e.ClientId).HasColumnName("client_id");
            entity.Property(e => e.Company)
                .HasMaxLength(191)
                .HasColumnName("company");
            entity.Property(e => e.Country).HasColumnName("country");
            entity.Property(e => e.DateAdded)
                .HasColumnType("datetime(3)")
                .HasColumnName("date_added");
            entity.Property(e => e.DateAssigned)
                .HasColumnType("datetime")
                .HasColumnName("date_assigned");
            entity.Property(e => e.DateConverted)
                .HasMaxLength(191)
                .HasColumnName("date_converted");
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(3)")
                .HasColumnType("datetime(3)")
                .HasColumnName("date_created");
            entity.Property(e => e.DefaultLanguage)
                .HasMaxLength(191)
                .HasColumnName("default_language");
            entity.Property(e => e.Description)
                .HasMaxLength(191)
                .HasColumnName("description");
            entity.Property(e => e.Email)
                .HasMaxLength(191)
                .HasColumnName("email");
            entity.Property(e => e.EmailIntegrationUid)
                .HasMaxLength(191)
                .HasColumnName("email_integration_uid");
            entity.Property(e => e.FromFormId).HasColumnName("from_form_id");
            entity.Property(e => e.Hash)
                .HasMaxLength(191)
                .HasColumnName("hash");
            entity.Property(e => e.IsImportedFromEmailIntegration).HasColumnName("is_imported_from_email_integration");
            entity.Property(e => e.IsPublic).HasColumnName("is_public");
            entity.Property(e => e.Junk).HasColumnName("junk");
            entity.Property(e => e.LastContact)
                .HasMaxLength(191)
                .HasColumnName("last_contact");
            entity.Property(e => e.LastLeadStatus).HasColumnName("last_lead_status");
            entity.Property(e => e.LastStatusChange)
                .HasMaxLength(191)
                .HasColumnName("last_status_change");
            entity.Property(e => e.LeadOrder).HasColumnName("lead_order");
            entity.Property(e => e.LeadValue).HasColumnName("lead_value");
            entity.Property(e => e.Lost).HasColumnName("lost");
            entity.Property(e => e.Name)
                .HasMaxLength(191)
                .HasColumnName("name");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(191)
                .HasColumnName("phone_number");
            entity.Property(e => e.SourceId).HasColumnName("source_id");
            entity.Property(e => e.State)
                .HasMaxLength(191)
                .HasColumnName("state");
            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.Title)
                .HasMaxLength(191)
                .HasColumnName("title");
            entity.Property(e => e.Website)
                .HasMaxLength(191)
                .HasColumnName("website");
            entity.Property(e => e.Zip)
                .HasMaxLength(191)
                .HasColumnName("zip");

            entity.HasOne(d => d.Source).WithMany(p => p.Leads)
                .HasForeignKey(d => d.SourceId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("leads_source_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.Leads)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("leads_status_id_fkey");
        });

        modelBuilder.Entity<LeadActivityLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("lead_activity_log")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.LeadId, "lead_activity_log_lead_id_fkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AdditionalData)
                .HasMaxLength(191)
                .HasColumnName("additional_data");
            entity.Property(e => e.CustomActivity).HasColumnName("custom_activity");
            entity.Property(e => e.Date)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(3)")
                .HasColumnType("datetime(3)")
                .HasColumnName("date");
            entity.Property(e => e.Description)
                .HasMaxLength(191)
                .HasColumnName("description");
            entity.Property(e => e.FullName)
                .HasMaxLength(191)
                .HasColumnName("full_name");
            entity.Property(e => e.LeadId).HasColumnName("lead_id");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");

            entity.HasOne(d => d.Lead).WithMany(p => p.LeadActivityLogs)
                .HasForeignKey(d => d.LeadId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("lead_activity_log_lead_id_fkey");
        });

        modelBuilder.Entity<LeadIntegrationEmail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("lead_integration_emails")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.LeadId, "lead_integration_emails_lead_id_fkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Body)
                .HasMaxLength(191)
                .HasColumnName("body");
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(3)")
                .HasColumnType("datetime(3)")
                .HasColumnName("date_created");
            entity.Property(e => e.Emailid).HasColumnName("emailid");
            entity.Property(e => e.LeadId).HasColumnName("lead_id");
            entity.Property(e => e.Subject)
                .HasMaxLength(191)
                .HasColumnName("subject");

            entity.HasOne(d => d.Lead).WithMany(p => p.LeadIntegrationEmails)
                .HasForeignKey(d => d.LeadId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("lead_integration_emails_lead_id_fkey");
        });

        modelBuilder.Entity<LeadsEmailIntegration>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("leads_email_integration")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.CheckEvery).HasColumnName("check_every");
            entity.Property(e => e.CreateTaskIfCustomer).HasColumnName("create_task_if_customer");
            entity.Property(e => e.DeleteAfterImport).HasColumnName("delete_after_import");
            entity.Property(e => e.Email)
                .HasMaxLength(191)
                .HasColumnName("email");
            entity.Property(e => e.Encryption)
                .HasMaxLength(191)
                .HasColumnName("encryption");
            entity.Property(e => e.Folder)
                .HasMaxLength(191)
                .HasColumnName("folder");
            entity.Property(e => e.ImapServer)
                .HasMaxLength(191)
                .HasColumnName("imap_server");
            entity.Property(e => e.LastRun)
                .HasMaxLength(191)
                .HasColumnName("last_run");
            entity.Property(e => e.LeadSource).HasColumnName("lead_source");
            entity.Property(e => e.LeadStatus).HasColumnName("lead_status");
            entity.Property(e => e.MarkPublic).HasColumnName("mark_public");
            entity.Property(e => e.NotifyIds)
                .HasMaxLength(191)
                .HasColumnName("notify_ids");
            entity.Property(e => e.NotifyLeadContactMoreTimes).HasColumnName("notify_lead_contact_more_times");
            entity.Property(e => e.NotifyLeadImported).HasColumnName("notify_lead_imported");
            entity.Property(e => e.NotifyType)
                .HasMaxLength(191)
                .HasColumnName("notify_type");
            entity.Property(e => e.OnlyLoopOnUnseenEmails).HasColumnName("only_loop_on_unseen_emails");
            entity.Property(e => e.Password)
                .HasMaxLength(191)
                .HasColumnName("password");
            entity.Property(e => e.Responsible).HasColumnName("responsible");
        });

        modelBuilder.Entity<LeadsSource>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("leads_sources")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(191)
                .HasColumnName("name");
        });

        modelBuilder.Entity<LeadsStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("leads_status")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Color)
                .HasMaxLength(191)
                .HasColumnName("color");
            entity.Property(e => e.IsDefault).HasColumnName("is_default");
            entity.Property(e => e.Lost)
                .HasDefaultValueSql("'0'")
                .HasColumnName("lost");
            entity.Property(e => e.Name)
                .HasMaxLength(191)
                .HasColumnName("name");
            entity.Property(e => e.StatusOrder).HasColumnName("status_order");
        });

        modelBuilder.Entity<MailQueue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("mail_queue")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AltMessage)
                .HasMaxLength(191)
                .HasColumnName("alt_message");
            entity.Property(e => e.Attachments)
                .HasMaxLength(191)
                .HasColumnName("attachments");
            entity.Property(e => e.Bcc)
                .HasMaxLength(191)
                .HasColumnName("bcc");
            entity.Property(e => e.Cc)
                .HasMaxLength(191)
                .HasColumnName("cc");
            entity.Property(e => e.Date)
                .HasColumnType("datetime(3)")
                .HasColumnName("date");
            entity.Property(e => e.Email)
                .HasMaxLength(191)
                .HasColumnName("email");
            entity.Property(e => e.Engine)
                .HasMaxLength(191)
                .HasColumnName("engine");
            entity.Property(e => e.Headers)
                .HasMaxLength(191)
                .HasColumnName("headers");
            entity.Property(e => e.Message)
                .HasMaxLength(191)
                .HasColumnName("message");
            entity.Property(e => e.Status)
                .HasMaxLength(191)
                .HasColumnName("status");
        });

        modelBuilder.Entity<Migration>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("migrations")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Version).HasColumnName("version");
        });

        modelBuilder.Entity<Milestone>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("milestones")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.ProjectId, "milestones_project_id_fkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Color)
                .HasMaxLength(191)
                .HasColumnName("color");
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(3)")
                .HasColumnType("datetime(3)")
                .HasColumnName("date_created");
            entity.Property(e => e.Description)
                .HasMaxLength(191)
                .HasColumnName("description");
            entity.Property(e => e.DescriptionVisibleToCustomer).HasColumnName("description_visible_to_customer");
            entity.Property(e => e.DueDate)
                .HasColumnType("datetime(3)")
                .HasColumnName("due_date");
            entity.Property(e => e.HideFromCustomer).HasColumnName("hide_from_customer");
            entity.Property(e => e.MilestoneOrder).HasColumnName("milestone_order");
            entity.Property(e => e.Name)
                .HasMaxLength(191)
                .HasColumnName("name");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.StartDate)
                .HasColumnType("datetime(3)")
                .HasColumnName("start_date");

            entity.HasOne(d => d.Project).WithMany(p => p.Milestones)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("milestones_project_id_fkey");
        });

        modelBuilder.Entity<Module>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("modules")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.InstalledVersion)
                .HasMaxLength(191)
                .HasColumnName("installed_version");
            entity.Property(e => e.ModuleName)
                .HasMaxLength(191)
                .HasColumnName("module_name");
        });

        modelBuilder.Entity<NewsfeedCommentLike>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("newsfeed_comment_likes")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.CommentId, "newsfeed_comment_likes_comment_id_fkey");

            entity.HasIndex(e => e.PostId, "newsfeed_comment_likes_post_id_fkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CommentId).HasColumnName("comment_id");
            entity.Property(e => e.DateLiked)
                .HasMaxLength(191)
                .HasColumnName("date_liked");
            entity.Property(e => e.PostId).HasColumnName("post_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Comment).WithMany(p => p.NewsfeedCommentLikes)
                .HasForeignKey(d => d.CommentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("newsfeed_comment_likes_comment_id_fkey");

            entity.HasOne(d => d.Post).WithMany(p => p.NewsfeedCommentLikes)
                .HasForeignKey(d => d.PostId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("newsfeed_comment_likes_post_id_fkey");
        });

        modelBuilder.Entity<NewsfeedPost>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("newsfeed_posts")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Content)
                .HasMaxLength(191)
                .HasColumnName("content");
            entity.Property(e => e.Creator).HasColumnName("creator");
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(3)")
                .HasColumnType("datetime(3)")
                .HasColumnName("date_created");
            entity.Property(e => e.DatePinned)
                .HasColumnType("datetime(3)")
                .HasColumnName("date_pinned");
            entity.Property(e => e.Pinned).HasColumnName("pinned");
            entity.Property(e => e.Visibility)
                .HasMaxLength(191)
                .HasColumnName("visibility");
        });

        modelBuilder.Entity<NewsfeedPostComment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("newsfeed_post_comments")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.PostId, "newsfeed_post_comments_post_id_fkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Content)
                .HasMaxLength(191)
                .HasColumnName("content");
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(3)")
                .HasColumnType("datetime(3)")
                .HasColumnName("date_created");
            entity.Property(e => e.PostId).HasColumnName("post_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Post).WithMany(p => p.NewsfeedPostComments)
                .HasForeignKey(d => d.PostId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("newsfeed_post_comments_post_id_fkey");
        });

        modelBuilder.Entity<NewsfeedPostLike>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("newsfeed_post_likes")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.PostId, "newsfeed_post_likes_post_id_fkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DateLiked)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(3)")
                .HasColumnType("datetime(3)")
                .HasColumnName("date_liked");
            entity.Property(e => e.PostId).HasColumnName("post_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Post).WithMany(p => p.NewsfeedPostLikes)
                .HasForeignKey(d => d.PostId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("newsfeed_post_likes_post_id_fkey");
        });

        modelBuilder.Entity<Note>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("notes")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AddedFrom).HasColumnName("added_from");
            entity.Property(e => e.DateContacted)
                .HasColumnType("datetime(3)")
                .HasColumnName("date_contacted");
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(3)")
                .HasColumnType("datetime(3)")
                .HasColumnName("date_created");
            entity.Property(e => e.Description)
                .HasMaxLength(191)
                .HasColumnName("description");
            entity.Property(e => e.RelId).HasColumnName("rel_id");
            entity.Property(e => e.RelType)
                .HasMaxLength(191)
                .HasColumnName("rel_type");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("notifications")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.FromClientId, "notifications_from_client_id_fkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AdditionalData)
                .HasMaxLength(191)
                .HasColumnName("additional_data");
            entity.Property(e => e.Date)
                .HasColumnType("datetime(3)")
                .HasColumnName("date");
            entity.Property(e => e.Description)
                .HasMaxLength(191)
                .HasColumnName("description");
            entity.Property(e => e.FromClientId).HasColumnName("from_client_id");
            entity.Property(e => e.FromCompany)
                .HasDefaultValueSql("'0'")
                .HasColumnName("from_company");
            entity.Property(e => e.FromFullname)
                .HasMaxLength(191)
                .HasColumnName("from_fullname");
            entity.Property(e => e.FromUserId).HasColumnName("from_user_id");
            entity.Property(e => e.IsRead).HasColumnName("is_read");
            entity.Property(e => e.IsReadInline).HasColumnName("is_read_inline");
            entity.Property(e => e.Link)
                .HasMaxLength(191)
                .HasColumnName("link");
            entity.Property(e => e.ToUserId).HasColumnName("to_user_id");

            entity.HasOne(d => d.FromClient).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.FromClientId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("notifications_from_client_id_fkey");
        });

        modelBuilder.Entity<Option>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("options")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Autoload).HasColumnName("autoload");
            entity.Property(e => e.Group)
                .HasMaxLength(191)
                .HasColumnName("group");
            entity.Property(e => e.Name)
                .HasMaxLength(191)
                .HasColumnName("name");
            entity.Property(e => e.Value)
                .HasMaxLength(191)
                .HasColumnName("value");
        });

        modelBuilder.Entity<PaymentAttempt>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("payment_attempts")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.InvoiceId, "payment_attempts_invoice_id_fkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(3)")
                .HasColumnType("datetime(3)")
                .HasColumnName("created_at");
            entity.Property(e => e.Fee).HasColumnName("fee");
            entity.Property(e => e.InvoiceId).HasColumnName("invoice_id");
            entity.Property(e => e.PaymentGateway)
                .HasMaxLength(191)
                .HasColumnName("payment_gateway");
            entity.Property(e => e.Reference)
                .HasMaxLength(191)
                .HasColumnName("reference");

            entity.HasOne(d => d.Invoice).WithMany(p => p.PaymentAttempts)
                .HasForeignKey(d => d.InvoiceId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("payment_attempts_invoice_id_fkey");
        });

        modelBuilder.Entity<PaymentMode>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("payment_modes")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.Description)
                .HasMaxLength(191)
                .HasColumnName("description");
            entity.Property(e => e.ExpensesOnly).HasColumnName("expenses_only");
            entity.Property(e => e.InvoicesOnly).HasColumnName("invoices_only");
            entity.Property(e => e.Name)
                .HasMaxLength(191)
                .HasColumnName("name");
            entity.Property(e => e.SelectedByDefault).HasColumnName("selected_by_default");
            entity.Property(e => e.ShowOnPdf).HasColumnName("show_on_pdf");
        });

        modelBuilder.Entity<PinnedProject>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("pinned_projects")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.ProjectId, "pinned_projects_project_id_fkey");

            entity.HasIndex(e => e.StaffId, "pinned_projects_staff_id_fkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");

            entity.HasOne(d => d.Project).WithMany(p => p.PinnedProjects)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pinned_projects_project_id_fkey");

            entity.HasOne(d => d.Staff).WithMany(p => p.PinnedProjects)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pinned_projects_staff_id_fkey");
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("projects")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.ClientId, "projects_client_id_fkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AddedFrom).HasColumnName("added_from");
            entity.Property(e => e.BillingType).HasColumnName("billing_type");
            entity.Property(e => e.ClientId).HasColumnName("client_id");
            entity.Property(e => e.ContactNotification).HasColumnName("contact_notification");
            entity.Property(e => e.DateFinished)
                .HasColumnType("datetime(3)")
                .HasColumnName("date_finished");
            entity.Property(e => e.Deadline)
                .HasColumnType("datetime(3)")
                .HasColumnName("deadline");
            entity.Property(e => e.Description)
                .HasMaxLength(191)
                .HasColumnName("description");
            entity.Property(e => e.EstimatedHours).HasColumnName("estimated_hours");
            entity.Property(e => e.Name)
                .HasMaxLength(191)
                .HasColumnName("name");
            entity.Property(e => e.NotifyContacts)
                .HasMaxLength(191)
                .HasColumnName("notify_contacts");
            entity.Property(e => e.Progress).HasColumnName("progress");
            entity.Property(e => e.ProgressFromTasks).HasColumnName("progress_from_tasks");
            entity.Property(e => e.ProjectCost).HasColumnName("project_cost");
            entity.Property(e => e.ProjectCreated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(3)")
                .HasColumnType("datetime(3)")
                .HasColumnName("project_created");
            entity.Property(e => e.ProjectRatePerHour).HasColumnName("project_rate_per_hour");
            entity.Property(e => e.StartDate)
                .HasColumnType("datetime(3)")
                .HasColumnName("start_date");
            entity.Property(e => e.Status).HasColumnName("status");

            entity.HasOne(d => d.Client).WithMany(p => p.Projects)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("projects_client_id_fkey");
        });

        modelBuilder.Entity<ProjectActivity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("project_activity")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.ContactId, "project_activity_contact_id_fkey");

            entity.HasIndex(e => e.ProjectId, "project_activity_project_id_fkey");

            entity.HasIndex(e => e.StaffId, "project_activity_staff_id_fkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AdditionalData)
                .HasMaxLength(191)
                .HasColumnName("additional_data");
            entity.Property(e => e.ContactId).HasColumnName("contact_id");
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(3)")
                .HasColumnType("datetime(3)")
                .HasColumnName("date_created");
            entity.Property(e => e.DescriptionKey)
                .HasMaxLength(191)
                .HasColumnName("description_key");
            entity.Property(e => e.FullName)
                .HasMaxLength(191)
                .HasColumnName("full_name");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");
            entity.Property(e => e.VisibleToCustomer).HasColumnName("visible_to_customer");

            entity.HasOne(d => d.Contact).WithMany(p => p.ProjectActivities)
                .HasForeignKey(d => d.ContactId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("project_activity_contact_id_fkey");

            entity.HasOne(d => d.Project).WithMany(p => p.ProjectActivities)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("project_activity_project_id_fkey");

            entity.HasOne(d => d.Staff).WithMany(p => p.ProjectActivities)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("project_activity_staff_id_fkey");
        });

        modelBuilder.Entity<ProjectDiscussion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("project_discussions")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.ContactId, "project_discussions_contact_id_fkey");

            entity.HasIndex(e => e.ProjectId, "project_discussions_project_id_fkey");

            entity.HasIndex(e => e.StaffId, "project_discussions_staff_id_fkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ContactId).HasColumnName("contact_id");
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("date_created");
            entity.Property(e => e.Description)
                .HasMaxLength(191)
                .HasColumnName("description");
            entity.Property(e => e.LastActivity)
                .HasColumnType("datetime")
                .HasColumnName("last_activity");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.ShowToCustomer).HasColumnName("show_to_customer");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");
            entity.Property(e => e.Subject)
                .HasMaxLength(191)
                .HasColumnName("subject");

            entity.HasOne(d => d.Contact).WithMany(p => p.ProjectDiscussions)
                .HasForeignKey(d => d.ContactId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("project_discussions_contact_id_fkey");

            entity.HasOne(d => d.Project).WithMany(p => p.ProjectDiscussions)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("project_discussions_project_id_fkey");

            entity.HasOne(d => d.Staff).WithMany(p => p.ProjectDiscussions)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("project_discussions_staff_id_fkey");
        });

        modelBuilder.Entity<ProjectDiscussionComment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("project_discussion_comments")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.ContactId, "project_discussion_comments_contact_id_fkey");

            entity.HasIndex(e => e.DiscussionId, "project_discussion_comments_discussion_id_fkey");

            entity.HasIndex(e => e.StaffId, "project_discussion_comments_staff_id_fkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ContactId).HasColumnName("contact_id");
            entity.Property(e => e.Content)
                .HasMaxLength(191)
                .HasColumnName("content");
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(3)")
                .HasColumnType("datetime(3)")
                .HasColumnName("date_created");
            entity.Property(e => e.DiscussionId).HasColumnName("discussion_id");
            entity.Property(e => e.DiscussionType)
                .HasMaxLength(191)
                .HasColumnName("discussion_type");
            entity.Property(e => e.FileMimeType)
                .HasMaxLength(191)
                .HasColumnName("file_mime_type");
            entity.Property(e => e.FileName)
                .HasMaxLength(191)
                .HasColumnName("file_name");
            entity.Property(e => e.FullName)
                .HasMaxLength(191)
                .HasColumnName("full_name");
            entity.Property(e => e.Modified)
                .HasMaxLength(191)
                .HasColumnName("modified");
            entity.Property(e => e.Parent).HasColumnName("parent");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");

            entity.HasOne(d => d.Contact).WithMany(p => p.ProjectDiscussionComments)
                .HasForeignKey(d => d.ContactId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("project_discussion_comments_contact_id_fkey");

            entity.HasOne(d => d.Discussion).WithMany(p => p.ProjectDiscussionComments)
                .HasForeignKey(d => d.DiscussionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("project_discussion_comments_discussion_id_fkey");

            entity.HasOne(d => d.Staff).WithMany(p => p.ProjectDiscussionComments)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("project_discussion_comments_staff_id_fkey");
        });

        modelBuilder.Entity<ProjectFile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("project_files")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.ContactId, "project_files_contact_id_fkey");

            entity.HasIndex(e => e.ProjectId, "project_files_project_id_fkey");

            entity.HasIndex(e => e.StaffId, "project_files_staff_id_fkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ContactId).HasColumnName("contact_id");
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(3)")
                .HasColumnType("datetime(3)")
                .HasColumnName("date_created");
            entity.Property(e => e.Description)
                .HasMaxLength(191)
                .HasColumnName("description");
            entity.Property(e => e.External)
                .HasMaxLength(191)
                .HasColumnName("external");
            entity.Property(e => e.ExternalLink)
                .HasMaxLength(191)
                .HasColumnName("external_link");
            entity.Property(e => e.FileName)
                .HasMaxLength(191)
                .HasColumnName("file_name");
            entity.Property(e => e.FileType)
                .HasMaxLength(191)
                .HasColumnName("file_type");
            entity.Property(e => e.LastActivity)
                .HasMaxLength(191)
                .HasColumnName("last_activity");
            entity.Property(e => e.OriginalFileName)
                .HasMaxLength(191)
                .HasColumnName("original_file_name");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");
            entity.Property(e => e.Subject)
                .HasMaxLength(191)
                .HasColumnName("subject");
            entity.Property(e => e.ThumbnailLink)
                .HasMaxLength(191)
                .HasColumnName("thumbnail_link");
            entity.Property(e => e.VisibleToCustomer).HasColumnName("visible_to_customer");

            entity.HasOne(d => d.Contact).WithMany(p => p.ProjectFiles)
                .HasForeignKey(d => d.ContactId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("project_files_contact_id_fkey");

            entity.HasOne(d => d.Project).WithMany(p => p.ProjectFiles)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("project_files_project_id_fkey");

            entity.HasOne(d => d.Staff).WithMany(p => p.ProjectFiles)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("project_files_staff_id_fkey");
        });

        modelBuilder.Entity<ProjectMember>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("project_members")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.ProjectId, "project_members_project_id_fkey");

            entity.HasIndex(e => e.StaffId, "project_members_staff_id_fkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");

            entity.HasOne(d => d.Project).WithMany(p => p.ProjectMembers)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("project_members_project_id_fkey");

            entity.HasOne(d => d.Staff).WithMany(p => p.ProjectMembers)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("project_members_staff_id_fkey");
        });

        modelBuilder.Entity<ProjectNote>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("project_notes")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.ProjectId, "project_notes_project_id_fkey");

            entity.HasIndex(e => e.StaffId, "project_notes_staff_id_fkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Content)
                .HasMaxLength(191)
                .HasColumnName("content");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");

            entity.HasOne(d => d.Project).WithMany(p => p.ProjectNotes)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("project_notes_project_id_fkey");

            entity.HasOne(d => d.Staff).WithMany(p => p.ProjectNotes)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("project_notes_staff_id_fkey");
        });

        modelBuilder.Entity<ProjectSetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("project_settings")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.ProjectId, "project_settings_project_id_fkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(191)
                .HasColumnName("name");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.Value)
                .HasMaxLength(191)
                .HasColumnName("value");

            entity.HasOne(d => d.Project).WithMany(p => p.ProjectSettings)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("project_settings_project_id_fkey");
        });

        modelBuilder.Entity<Proposal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("proposals")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.CountryId, "proposals_country_id_fkey");

            entity.HasIndex(e => e.EstimateId, "proposals_estimate_id_fkey");

            entity.HasIndex(e => e.InvoiceId, "proposals_invoice_id_fkey");

            entity.HasIndex(e => e.ProjectId, "proposals_project_id_fkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AcceptanceDate)
                .HasMaxLength(191)
                .HasColumnName("acceptance_date");
            entity.Property(e => e.AcceptanceEmail).HasColumnName("acceptance_email");
            entity.Property(e => e.AcceptanceFirstName)
                .HasMaxLength(191)
                .HasColumnName("acceptance_first_name");
            entity.Property(e => e.AcceptanceIp)
                .HasMaxLength(191)
                .HasColumnName("acceptance_ip");
            entity.Property(e => e.AcceptanceLastName)
                .HasMaxLength(191)
                .HasColumnName("acceptance_last_name");
            entity.Property(e => e.AddedFrom).HasColumnName("added_from");
            entity.Property(e => e.Address)
                .HasMaxLength(191)
                .HasColumnName("address");
            entity.Property(e => e.Adjustment).HasColumnName("adjustment");
            entity.Property(e => e.AllowComments).HasColumnName("allow_comments");
            entity.Property(e => e.Assigned).HasColumnName("assigned");
            entity.Property(e => e.City)
                .HasMaxLength(191)
                .HasColumnName("city");
            entity.Property(e => e.Content)
                .HasMaxLength(191)
                .HasColumnName("content");
            entity.Property(e => e.CountryId).HasColumnName("country_id");
            entity.Property(e => e.Currency).HasColumnName("currency");
            entity.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasColumnName("date");
            entity.Property(e => e.DateConverted)
                .HasColumnType("datetime(3)")
                .HasColumnName("date_converted");
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(3)")
                .HasColumnType("datetime(3)")
                .HasColumnName("date_created");
            entity.Property(e => e.DiscountPercent).HasColumnName("discount_percent");
            entity.Property(e => e.DiscountTotal).HasColumnName("discount_total");
            entity.Property(e => e.DiscountType)
                .HasMaxLength(191)
                .HasColumnName("discount_type");
            entity.Property(e => e.Email)
                .HasMaxLength(191)
                .HasColumnName("email");
            entity.Property(e => e.EstimateId).HasColumnName("estimate_id");
            entity.Property(e => e.Hash)
                .HasMaxLength(191)
                .HasColumnName("hash");
            entity.Property(e => e.InvoiceId).HasColumnName("invoice_id");
            entity.Property(e => e.IsExpiryNotified).HasColumnName("is_expiry_notified");
            entity.Property(e => e.OpenTill)
                .HasColumnType("datetime")
                .HasColumnName("open_till");
            entity.Property(e => e.Phone)
                .HasMaxLength(191)
                .HasColumnName("phone");
            entity.Property(e => e.PipelineOrder).HasColumnName("pipeline_order");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.ProposalTo)
                .HasMaxLength(191)
                .HasColumnName("proposal_to");
            entity.Property(e => e.RelId).HasColumnName("rel_id");
            entity.Property(e => e.RelType)
                .HasMaxLength(191)
                .HasColumnName("rel_type");
            entity.Property(e => e.ShortLink)
                .HasMaxLength(191)
                .HasColumnName("short_link");
            entity.Property(e => e.ShowQuantityAs).HasColumnName("show_quantity_as");
            entity.Property(e => e.Signature)
                .HasMaxLength(191)
                .HasColumnName("signature");
            entity.Property(e => e.State)
                .HasMaxLength(191)
                .HasColumnName("state");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.Subject)
                .HasMaxLength(191)
                .HasColumnName("subject");
            entity.Property(e => e.Subtotal).HasColumnName("subtotal");
            entity.Property(e => e.Total).HasColumnName("total");
            entity.Property(e => e.TotalTax).HasColumnName("total_tax");
            entity.Property(e => e.Zip)
                .HasMaxLength(191)
                .HasColumnName("zip");

            entity.HasOne(d => d.Country).WithMany(p => p.Proposals)
                .HasForeignKey(d => d.CountryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("proposals_country_id_fkey");

            entity.HasOne(d => d.Estimate).WithMany(p => p.Proposals)
                .HasForeignKey(d => d.EstimateId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("proposals_estimate_id_fkey");

            entity.HasOne(d => d.Invoice).WithMany(p => p.Proposals)
                .HasForeignKey(d => d.InvoiceId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("proposals_invoice_id_fkey");

            entity.HasOne(d => d.Project).WithMany(p => p.Proposals)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("proposals_project_id_fkey");
        });

        modelBuilder.Entity<ProposalComment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("proposal_comments")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.ProposalId, "proposal_comments_proposal_id_fkey");

            entity.HasIndex(e => e.StaffId, "proposal_comments_staff_id_fkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Content)
                .HasMaxLength(191)
                .HasColumnName("content");
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(3)")
                .HasColumnType("datetime(3)")
                .HasColumnName("date_created");
            entity.Property(e => e.ProposalId).HasColumnName("proposal_id");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");

            entity.HasOne(d => d.Proposal).WithMany(p => p.ProposalComments)
                .HasForeignKey(d => d.ProposalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("proposal_comments_proposal_id_fkey");

            entity.HasOne(d => d.Staff).WithMany(p => p.ProposalComments)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("proposal_comments_staff_id_fkey");
        });

        modelBuilder.Entity<RelatedItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("related_items")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ItemId).HasColumnName("item_id");
            entity.Property(e => e.RelId).HasColumnName("rel_id");
            entity.Property(e => e.RelType)
                .HasMaxLength(191)
                .HasColumnName("rel_type");
        });

        modelBuilder.Entity<Reminder>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("reminders")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.StaffId, "reminders_staff_id_fkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Creator).HasColumnName("creator");
            entity.Property(e => e.Date)
                .HasColumnType("datetime(3)")
                .HasColumnName("date");
            entity.Property(e => e.Description)
                .HasMaxLength(191)
                .HasColumnName("description");
            entity.Property(e => e.IsNotified).HasColumnName("is_notified");
            entity.Property(e => e.NotifyByEmail).HasColumnName("notify_by_email");
            entity.Property(e => e.RelId).HasColumnName("rel_id");
            entity.Property(e => e.RelType)
                .HasMaxLength(191)
                .HasColumnName("rel_type");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");

            entity.HasOne(d => d.Staff).WithMany(p => p.Reminders)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("reminders_staff_id_fkey");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("roles")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(191)
                .HasColumnName("name");
            entity.Property(e => e.Permissions)
                .HasMaxLength(191)
                .HasColumnName("permissions");
        });

        modelBuilder.Entity<SalesActivity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("sales_activity")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.StaffId1, "sales_activity_staffId_fkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AdditionalData)
                .HasMaxLength(191)
                .HasColumnName("additional_data");
            entity.Property(e => e.Date)
                .HasColumnType("datetime(3)")
                .HasColumnName("date");
            entity.Property(e => e.Description)
                .HasMaxLength(191)
                .HasColumnName("description");
            entity.Property(e => e.FullName)
                .HasMaxLength(191)
                .HasColumnName("full_name");
            entity.Property(e => e.RelId).HasColumnName("rel_id");
            entity.Property(e => e.RelType)
                .HasMaxLength(191)
                .HasColumnName("rel_type");
            entity.Property(e => e.StaffId)
                .HasMaxLength(191)
                .HasColumnName("staff_id");
            entity.Property(e => e.StaffId1).HasColumnName("staffId");

            entity.HasOne(d => d.StaffId1Navigation).WithMany(p => p.SalesActivities)
                .HasForeignKey(d => d.StaffId1)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("sales_activity_staffId_fkey");
        });

        modelBuilder.Entity<ScheduledEmail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("scheduled_emails")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AttachPdf).HasColumnName("attach_pdf");
            entity.Property(e => e.Cc)
                .HasMaxLength(191)
                .HasColumnName("cc");
            entity.Property(e => e.Contacts)
                .HasMaxLength(191)
                .HasColumnName("contacts");
            entity.Property(e => e.RelId).HasColumnName("rel_id");
            entity.Property(e => e.RelType)
                .HasMaxLength(191)
                .HasColumnName("rel_type");
            entity.Property(e => e.ScheduledAt)
                .HasColumnType("datetime(3)")
                .HasColumnName("scheduled_at");
            entity.Property(e => e.Template)
                .HasMaxLength(191)
                .HasColumnName("template");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("services")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(191)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("sessions")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Data)
                .HasMaxLength(191)
                .HasColumnName("data");
            entity.Property(e => e.ExpiresAt)
                .HasColumnType("datetime(3)")
                .HasColumnName("expires_at");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(191)
                .HasColumnName("ip_address");
            entity.Property(e => e.IsSerialize).HasColumnName("is_serialize");
            entity.Property(e => e.Uuid)
                .HasMaxLength(191)
                .HasColumnName("uuid");
        });

        modelBuilder.Entity<SharedCustomerFile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("shared_customer_files")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.ContactId, "shared_customer_files_contact_id_fkey");

            entity.HasIndex(e => e.FileId, "shared_customer_files_file_id_fkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ContactId).HasColumnName("contact_id");
            entity.Property(e => e.FileId).HasColumnName("file_id");

            entity.HasOne(d => d.Contact).WithMany(p => p.SharedCustomerFiles)
                .HasForeignKey(d => d.ContactId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("shared_customer_files_contact_id_fkey");

            entity.HasOne(d => d.File).WithMany(p => p.SharedCustomerFiles)
                .HasPrincipalKey(p => p.Id)
                .HasForeignKey(d => d.FileId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("shared_customer_files_file_id_fkey");
        });

        modelBuilder.Entity<SpamFilter>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("spam_filters")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.RelType)
                .HasMaxLength(191)
                .HasColumnName("rel_type");
            entity.Property(e => e.Type)
                .HasMaxLength(191)
                .HasColumnName("type");
            entity.Property(e => e.Value)
                .HasMaxLength(191)
                .HasColumnName("value");
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("staff")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Active)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("active");
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(3)")
                .HasColumnType("datetime(3)")
                .HasColumnName("date_created");
            entity.Property(e => e.DefaultLanguage)
                .HasMaxLength(191)
                .HasDefaultValueSql("'en'")
                .HasColumnName("default_language");
            entity.Property(e => e.Email)
                .HasMaxLength(191)
                .HasColumnName("email");
            entity.Property(e => e.EmailSignature)
                .HasMaxLength(191)
                .HasColumnName("email_signature");
            entity.Property(e => e.Facebook)
                .HasMaxLength(191)
                .HasColumnName("facebook");
            entity.Property(e => e.FirstName)
                .HasMaxLength(191)
                .HasColumnName("first_name");
            entity.Property(e => e.GoogleAuthSecret)
                .HasMaxLength(191)
                .HasColumnName("google_auth_secret");
            entity.Property(e => e.HourlyRate).HasColumnName("hourly_rate");
            entity.Property(e => e.IsAdmin).HasColumnName("is_admin");
            entity.Property(e => e.IsNotStaff).HasColumnName("is_not_staff");
            entity.Property(e => e.LastActivity)
                .HasMaxLength(191)
                .HasColumnName("last_activity");
            entity.Property(e => e.LastIp)
                .HasMaxLength(191)
                .HasColumnName("last_ip");
            entity.Property(e => e.LastLogin)
                .HasColumnType("datetime(3)")
                .HasColumnName("last_login");
            entity.Property(e => e.LastName)
                .HasMaxLength(191)
                .HasColumnName("last_name");
            entity.Property(e => e.LastPasswordChange)
                .HasColumnType("datetime(3)")
                .HasColumnName("last_password_change");
            entity.Property(e => e.LinkedIn)
                .HasMaxLength(191)
                .HasColumnName("linked_in");
            entity.Property(e => e.MediaPathSlug)
                .HasMaxLength(191)
                .HasColumnName("media_path_slug");
            entity.Property(e => e.NewPassKey)
                .HasMaxLength(191)
                .HasColumnName("new_pass_key");
            entity.Property(e => e.NewPassKeyRequested)
                .HasMaxLength(191)
                .HasColumnName("new_pass_key_requested");
            entity.Property(e => e.Password)
                .HasMaxLength(191)
                .HasColumnName("password");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(191)
                .HasColumnName("phone_number");
            entity.Property(e => e.ProfileImage)
                .HasMaxLength(191)
                .HasColumnName("profile_image");
            entity.Property(e => e.Role).HasColumnName("role");
            entity.Property(e => e.Skype)
                .HasMaxLength(191)
                .HasColumnName("skype");
            entity.Property(e => e.TwoFactorAuthCode)
                .HasMaxLength(191)
                .HasColumnName("two_factor_auth_code");
            entity.Property(e => e.TwoFactorAuthCodeRequested)
                .HasColumnType("datetime(3)")
                .HasColumnName("two_factor_auth_code_requested");
            entity.Property(e => e.TwoFactorAuthEnabled).HasColumnName("two_factor_auth_enabled");
            entity.Property(e => e.Uuid)
                .HasMaxLength(191)
                .HasColumnName("uuid");
        });

        modelBuilder.Entity<StaffDepartment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("staff_departments")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.DepartmentId, "staff_departments_department_id_fkey");

            entity.HasIndex(e => e.StaffId, "staff_departments_staff_id_fkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");

            entity.HasOne(d => d.Department).WithMany(p => p.StaffDepartments)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("staff_departments_department_id_fkey");

            entity.HasOne(d => d.Staff).WithMany(p => p.StaffDepartments)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("staff_departments_staff_id_fkey");
        });

        modelBuilder.Entity<StaffPermission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("staff_permissions")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.StaffId, "staff_permissions_staff_id_fkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Capability)
                .HasMaxLength(191)
                .HasColumnName("capability");
            entity.Property(e => e.Feature)
                .HasMaxLength(191)
                .HasColumnName("feature");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");

            entity.HasOne(d => d.Staff).WithMany(p => p.StaffPermissions)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("staff_permissions_staff_id_fkey");
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("subscriptions")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.ClientId, "subscriptions_client_id_fkey");

            entity.HasIndex(e => e.ProjectId, "subscriptions_project_id_fkey");

            entity.HasIndex(e => e.TaxId, "tax_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClientId).HasColumnName("client_id");
            entity.Property(e => e.CreatedFrom).HasColumnName("created_from");
            entity.Property(e => e.Currency).HasColumnName("currency");
            entity.Property(e => e.Date)
                .HasColumnType("datetime(3)")
                .HasColumnName("date");
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(3)")
                .HasColumnType("datetime(3)")
                .HasColumnName("date_created");
            entity.Property(e => e.DateSubscribed)
                .HasColumnType("datetime(3)")
                .HasColumnName("date_subscribed");
            entity.Property(e => e.Description)
                .HasMaxLength(191)
                .HasColumnName("description");
            entity.Property(e => e.DescriptionInItem).HasColumnName("description_in_item");
            entity.Property(e => e.EndsAt)
                .HasColumnType("datetime(3)")
                .HasColumnName("ends_at");
            entity.Property(e => e.Hash)
                .HasMaxLength(191)
                .HasColumnName("hash");
            entity.Property(e => e.InTestEnvironment).HasColumnName("in_test_environment");
            entity.Property(e => e.LastSentAt)
                .HasColumnType("datetime(3)")
                .HasColumnName("last_sent_at");
            entity.Property(e => e.Name)
                .HasMaxLength(191)
                .HasColumnName("name");
            entity.Property(e => e.NextBillingCycle).HasColumnName("next_billing_cycle");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.Status)
                .HasMaxLength(191)
                .HasColumnName("status");
            entity.Property(e => e.StripePlanId)
                .HasMaxLength(191)
                .HasColumnName("stripe_plan_id");
            entity.Property(e => e.StripeSubscriptionId)
                .HasMaxLength(191)
                .HasColumnName("stripe_subscription_id");
            entity.Property(e => e.StripeTaxId)
                .HasMaxLength(191)
                .HasColumnName("stripe_tax_id");
            entity.Property(e => e.StripeTaxId2)
                .HasMaxLength(191)
                .HasColumnName("stripe_tax_id_2");
            entity.Property(e => e.TaxId).HasColumnName("tax_id");
            entity.Property(e => e.TaxId2).HasColumnName("tax_id_2");
            entity.Property(e => e.Terms)
                .HasMaxLength(191)
                .HasColumnName("terms");

            entity.HasOne(d => d.Client).WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("subscriptions_client_id_fkey");

            entity.HasOne(d => d.Project).WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("subscriptions_project_id_fkey");

            entity.HasOne(d => d.Tax).WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.TaxId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("subscriptions_tax_id_fkey");
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("tags")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(191)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Taggable>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("taggables")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.TagId, "tag_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.RelId).HasColumnName("rel_id");
            entity.Property(e => e.RelType)
                .HasMaxLength(191)
                .HasColumnName("rel_type");
            entity.Property(e => e.TagId).HasColumnName("tag_id");
            entity.Property(e => e.TagOrder).HasColumnName("tag_order");
        });

        modelBuilder.Entity<Task>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("tasks")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.KanbanOrder, "kanban_order");

            entity.HasIndex(e => e.Milestone, "milestone");

            entity.HasIndex(e => e.InvoiceId, "tasks_invoice_id_fkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AddedFrom).HasColumnName("added_from");
            entity.Property(e => e.Billable).HasColumnName("billable");
            entity.Property(e => e.Billed).HasColumnName("billed");
            entity.Property(e => e.CustomRecurring).HasColumnName("custom_recurring");
            entity.Property(e => e.Cycles).HasColumnName("cycles");
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("date_created");
            entity.Property(e => e.DateFinished)
                .HasColumnType("datetime")
                .HasColumnName("date_finished");
            entity.Property(e => e.DeadlineNotified).HasColumnName("deadline_notified");
            entity.Property(e => e.Description)
                .HasMaxLength(191)
                .HasColumnName("description");
            entity.Property(e => e.DueDate)
                .HasColumnType("datetime")
                .HasColumnName("due_date");
            entity.Property(e => e.HourlyRate).HasColumnName("hourly_rate");
            entity.Property(e => e.InvoiceId).HasColumnName("invoice_id");
            entity.Property(e => e.IsAddedFromContact).HasColumnName("is_added_from_contact");
            entity.Property(e => e.IsPublic).HasColumnName("is_public");
            entity.Property(e => e.IsRecurringFrom).HasColumnName("is_recurring_from");
            entity.Property(e => e.KanbanOrder).HasColumnName("kanban_order");
            entity.Property(e => e.LastRecurringDate)
                .HasMaxLength(191)
                .HasColumnName("last_recurring_date");
            entity.Property(e => e.Milestone).HasColumnName("milestone");
            entity.Property(e => e.MilestoneOrder).HasColumnName("milestone_order");
            entity.Property(e => e.Name)
                .HasMaxLength(191)
                .HasColumnName("name");
            entity.Property(e => e.Priority).HasColumnName("priority");
            entity.Property(e => e.Recurring).HasColumnName("recurring");
            entity.Property(e => e.RecurringType)
                .HasMaxLength(191)
                .HasColumnName("recurring_type");
            entity.Property(e => e.RelId).HasColumnName("rel_id");
            entity.Property(e => e.RelType)
                .HasMaxLength(191)
                .HasColumnName("rel_type");
            entity.Property(e => e.RepeatEvery)
                .HasMaxLength(191)
                .HasColumnName("repeat_every");
            entity.Property(e => e.StartDate)
                .HasColumnType("datetime")
                .HasColumnName("start_date");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.TotalCycles).HasColumnName("total_cycles");
            entity.Property(e => e.VisibleToClient).HasColumnName("visible_to_client");

            entity.HasOne(d => d.Invoice).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.InvoiceId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("tasks_invoice_id_fkey");
        });

        modelBuilder.Entity<TaskAssigned>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("task_assigned")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.StaffId, "task_assigned_staff_id_fkey");

            entity.HasIndex(e => e.TaskId, "taskid");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AssignedFrom).HasColumnName("assigned_from");
            entity.Property(e => e.IsAssignedFromContact).HasColumnName("is_assigned_from_contact");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");
            entity.Property(e => e.TaskId).HasColumnName("task_id");

            entity.HasOne(d => d.Staff).WithMany(p => p.TaskAssigneds)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("task_assigned_staff_id_fkey");

            entity.HasOne(d => d.Task).WithMany(p => p.TaskAssigneds)
                .HasForeignKey(d => d.TaskId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("task_assigned_task_id_fkey");
        });

        modelBuilder.Entity<TaskChecklistItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("task_checklist_items")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.TaskId, "task_checklist_items_task_id_fkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AddedFrom).HasColumnName("added_from");
            entity.Property(e => e.Assigned).HasColumnName("assigned");
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(3)")
                .HasColumnType("datetime(3)")
                .HasColumnName("date_created");
            entity.Property(e => e.Description)
                .HasMaxLength(191)
                .HasColumnName("description");
            entity.Property(e => e.Finished).HasColumnName("finished");
            entity.Property(e => e.FinishedFrom).HasColumnName("finished_from");
            entity.Property(e => e.ListOrder)
                .HasMaxLength(255)
                .HasColumnName("list_order");
            entity.Property(e => e.TaskId).HasColumnName("task_id");

            entity.HasOne(d => d.Task).WithMany(p => p.TaskChecklistItems)
                .HasForeignKey(d => d.TaskId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("task_checklist_items_task_id_fkey");
        });

        modelBuilder.Entity<TaskComment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("task_comments")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.FileId, "file_id");

            entity.HasIndex(e => e.ContactId, "task_comments_contact_id_fkey");

            entity.HasIndex(e => e.StaffId, "task_comments_staff_id_fkey");

            entity.HasIndex(e => e.TaskId, "task_comments_task_id_fkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ContactId).HasColumnName("contact_id");
            entity.Property(e => e.Content)
                .HasMaxLength(191)
                .HasColumnName("content");
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(3)")
                .HasColumnType("datetime(3)")
                .HasColumnName("date_created");
            entity.Property(e => e.FileId).HasColumnName("file_id");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");
            entity.Property(e => e.TaskId).HasColumnName("task_id");

            entity.HasOne(d => d.Contact).WithMany(p => p.TaskComments)
                .HasForeignKey(d => d.ContactId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("task_comments_contact_id_fkey");

            entity.HasOne(d => d.File).WithMany(p => p.TaskComments)
                .HasPrincipalKey(p => p.Id)
                .HasForeignKey(d => d.FileId)
                .HasConstraintName("task_comments_file_id_fkey");

            entity.HasOne(d => d.Staff).WithMany(p => p.TaskComments)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("task_comments_staff_id_fkey");

            entity.HasOne(d => d.Task).WithMany(p => p.TaskComments)
                .HasForeignKey(d => d.TaskId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("task_comments_task_id_fkey");
        });

        modelBuilder.Entity<TaskFollower>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("task_followers")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.StaffId, "task_followers_staff_id_fkey");

            entity.HasIndex(e => e.TaskId, "task_followers_task_id_fkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");
            entity.Property(e => e.TaskId).HasColumnName("task_id");

            entity.HasOne(d => d.Staff).WithMany(p => p.TaskFollowers)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("task_followers_staff_id_fkey");

            entity.HasOne(d => d.Task).WithMany(p => p.TaskFollowers)
                .HasForeignKey(d => d.TaskId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("task_followers_task_id_fkey");
        });

        modelBuilder.Entity<TasksChecklistTemplate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("tasks_checklist_templates")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(191)
                .HasColumnName("description");
        });

        modelBuilder.Entity<TasksTimer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("tasks_timers")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.TaskId, "task_id");

            entity.HasIndex(e => e.StaffId, "tasks_timers_staff_id_fkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EndTime)
                .HasColumnType("datetime")
                .HasColumnName("end_time");
            entity.Property(e => e.HourlyRate).HasColumnName("hourly_rate");
            entity.Property(e => e.Note)
                .HasMaxLength(191)
                .HasColumnName("note");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");
            entity.Property(e => e.StartTime)
                .HasColumnType("datetime")
                .HasColumnName("start_time");
            entity.Property(e => e.TaskId).HasColumnName("task_id");

            entity.HasOne(d => d.Staff).WithMany(p => p.TasksTimers)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tasks_timers_staff_id_fkey");

            entity.HasOne(d => d.Task).WithMany(p => p.TasksTimers)
                .HasForeignKey(d => d.TaskId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tasks_timers_task_id_fkey");
        });

        modelBuilder.Entity<Taxis>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("taxes")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(191)
                .HasColumnName("name");
            entity.Property(e => e.TaxRate)
                .HasMaxLength(191)
                .HasColumnName("tax_rate");
        });

        modelBuilder.Entity<Template>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("templates")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AddedFrom).HasColumnName("added_from");
            entity.Property(e => e.Content)
                .HasMaxLength(191)
                .HasColumnName("content");
            entity.Property(e => e.Name)
                .HasMaxLength(191)
                .HasColumnName("name");
            entity.Property(e => e.Type)
                .HasMaxLength(191)
                .HasColumnName("type");
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.MergedTicketId })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity
                .ToTable("tickets")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.ContactId, "contactid");

            entity.HasIndex(e => e.DepartmentId, "department");

            entity.HasIndex(e => e.Priority, "priority");

            entity.HasIndex(e => e.Service, "service");

            entity.HasIndex(e => e.Id, "tickets_id_key").IsUnique();

            entity.HasIndex(e => e.ProjectId, "tickets_project_id_fkey");

            entity.HasIndex(e => e.ResponderId, "tickets_responder_id_fkey");

            entity.HasIndex(e => e.Status, "tickets_status_fkey");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");
            entity.Property(e => e.MergedTicketId).HasColumnName("merged_ticket_id");
            entity.Property(e => e.Admin).HasColumnName("admin");
            entity.Property(e => e.AdminRead).HasColumnName("admin_read");
            entity.Property(e => e.AdminReplying).HasColumnName("admin_replying");
            entity.Property(e => e.Assigned).HasColumnName("assigned");
            entity.Property(e => e.Cc)
                .HasMaxLength(191)
                .HasColumnName("cc");
            entity.Property(e => e.ClientRead).HasColumnName("client_read");
            entity.Property(e => e.ContactId).HasColumnName("contact_id");
            entity.Property(e => e.Date)
                .HasMaxLength(191)
                .HasColumnName("date");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.Email)
                .HasMaxLength(191)
                .HasColumnName("email");
            entity.Property(e => e.LastReply)
                .HasColumnType("datetime")
                .HasColumnName("last_reply");
            entity.Property(e => e.Message)
                .HasMaxLength(191)
                .HasColumnName("message");
            entity.Property(e => e.Name)
                .HasMaxLength(191)
                .HasColumnName("name");
            entity.Property(e => e.Priority).HasColumnName("priority");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.ResponderId).HasColumnName("responder_id");
            entity.Property(e => e.Service).HasColumnName("service");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.Subject)
                .HasMaxLength(191)
                .HasColumnName("subject");
            entity.Property(e => e.TicketKey)
                .HasMaxLength(191)
                .HasColumnName("ticket_key");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Contact).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.ContactId)
                .HasConstraintName("tickets_contact_id_fkey");

            entity.HasOne(d => d.Department).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("tickets_department_id_fkey");

            entity.HasOne(d => d.Project).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.ProjectId)
                .HasConstraintName("tickets_project_id_fkey");

            entity.HasOne(d => d.Responder).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.ResponderId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("tickets_responder_id_fkey");

            entity.HasOne(d => d.StatusNavigation).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.Status)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("tickets_status_fkey");
        });

        modelBuilder.Entity<TicketAttachment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("ticket_attachments")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.TicketId, "ticket_attachments_ticket_id_fkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(3)")
                .HasColumnType("datetime(3)")
                .HasColumnName("date_created");
            entity.Property(e => e.FileName)
                .HasMaxLength(191)
                .HasColumnName("file_name");
            entity.Property(e => e.FileType)
                .HasMaxLength(191)
                .HasColumnName("file_type");
            entity.Property(e => e.ReplyId).HasColumnName("reply_id");
            entity.Property(e => e.TicketId).HasColumnName("ticket_id");

            entity.HasOne(d => d.Ticket).WithMany(p => p.TicketAttachments)
                .HasPrincipalKey(p => p.Id)
                .HasForeignKey(d => d.TicketId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ticket_attachments_ticket_id_fkey");
        });

        modelBuilder.Entity<TicketReply>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("ticket_replies")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.Admin, "ticket_replies_admin_fkey");

            entity.HasIndex(e => e.ContactId, "ticket_replies_contact_id_fkey");

            entity.HasIndex(e => e.TicketId, "ticket_replies_ticket_id_fkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Admin).HasColumnName("admin");
            entity.Property(e => e.Attachment).HasColumnName("attachment");
            entity.Property(e => e.ContactId).HasColumnName("contact_id");
            entity.Property(e => e.Date)
                .HasColumnType("datetime(3)")
                .HasColumnName("date");
            entity.Property(e => e.Email)
                .HasMaxLength(191)
                .HasColumnName("email");
            entity.Property(e => e.Message)
                .HasMaxLength(191)
                .HasColumnName("message");
            entity.Property(e => e.Name)
                .HasMaxLength(191)
                .HasColumnName("name");
            entity.Property(e => e.TicketId).HasColumnName("ticket_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.AdminNavigation).WithMany(p => p.TicketReplies)
                .HasForeignKey(d => d.Admin)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("ticket_replies_admin_fkey");

            entity.HasOne(d => d.Contact).WithMany(p => p.TicketReplies)
                .HasForeignKey(d => d.ContactId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ticket_replies_contact_id_fkey");

            entity.HasOne(d => d.Ticket).WithMany(p => p.TicketReplies)
                .HasPrincipalKey(p => p.Id)
                .HasForeignKey(d => d.TicketId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ticket_replies_ticket_id_fkey");
        });

        modelBuilder.Entity<TicketsPipeLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("tickets_pipe_log")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(3)")
                .HasColumnType("datetime(3)")
                .HasColumnName("date_created");
            entity.Property(e => e.Email)
                .HasMaxLength(191)
                .HasColumnName("email");
            entity.Property(e => e.EmailTo)
                .HasMaxLength(191)
                .HasColumnName("email_to");
            entity.Property(e => e.Message)
                .HasMaxLength(191)
                .HasColumnName("message");
            entity.Property(e => e.Name)
                .HasMaxLength(191)
                .HasColumnName("name");
            entity.Property(e => e.Status)
                .HasMaxLength(191)
                .HasColumnName("status");
            entity.Property(e => e.Subject)
                .HasMaxLength(191)
                .HasColumnName("subject");
        });

        modelBuilder.Entity<TicketsPredefinedReply>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("tickets_predefined_replies")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Message)
                .HasMaxLength(191)
                .HasColumnName("message");
            entity.Property(e => e.Name)
                .HasMaxLength(191)
                .HasColumnName("name");
        });

        modelBuilder.Entity<TicketsPriority>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("tickets_priorities")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(191)
                .HasColumnName("name");
        });

        modelBuilder.Entity<TicketsStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("tickets_status")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IsDefault).HasColumnName("is_default");
            entity.Property(e => e.Name)
                .HasMaxLength(191)
                .HasColumnName("name");
            entity.Property(e => e.StatusColor)
                .HasMaxLength(191)
                .HasColumnName("status_color");
            entity.Property(e => e.StatusOrder).HasColumnName("status_order");
        });

        modelBuilder.Entity<Todo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("todos")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.StaffId, "todos_staff_id_fkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(3)")
                .HasColumnType("datetime(3)")
                .HasColumnName("date_created");
            entity.Property(e => e.DateFinished)
                .HasMaxLength(191)
                .HasColumnName("date_finished");
            entity.Property(e => e.Description)
                .HasMaxLength(191)
                .HasColumnName("description");
            entity.Property(e => e.Finished).HasColumnName("finished");
            entity.Property(e => e.ItemOrder).HasColumnName("item_order");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");

            entity.HasOne(d => d.Staff).WithMany(p => p.Todos)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("todos_staff_id_fkey");
        });

        modelBuilder.Entity<TrackedMail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("tracked_mails")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(3)")
                .HasColumnType("datetime(3)")
                .HasColumnName("date_created");
            entity.Property(e => e.DateOpened)
                .HasColumnType("datetime(3)")
                .HasColumnName("date_opened");
            entity.Property(e => e.Email)
                .HasMaxLength(191)
                .HasColumnName("email");
            entity.Property(e => e.Opened).HasColumnName("opened");
            entity.Property(e => e.RelId).HasColumnName("rel_id");
            entity.Property(e => e.RelType)
                .HasMaxLength(191)
                .HasColumnName("rel_type");
            entity.Property(e => e.Subject)
                .HasMaxLength(191)
                .HasColumnName("subject");
            entity.Property(e => e.Uid)
                .HasMaxLength(191)
                .HasColumnName("uid");
        });

        modelBuilder.Entity<TwoCheckoutLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("two_checkout_log")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.InvoiceId, "invoice_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasMaxLength(191)
                .HasColumnName("amount");
            entity.Property(e => e.AttemptReference)
                .HasMaxLength(191)
                .HasColumnName("attempt_reference");
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(3)")
                .HasColumnType("datetime(3)")
                .HasColumnName("date_created");
            entity.Property(e => e.InvoiceId).HasColumnName("invoice_id");
            entity.Property(e => e.Reference)
                .HasMaxLength(191)
                .HasColumnName("reference");

            entity.HasOne(d => d.Invoice).WithMany(p => p.TwoCheckoutLogs)
                .HasForeignKey(d => d.InvoiceId)
                .HasConstraintName("two_checkout_log_invoice_id_fkey");
        });

        modelBuilder.Entity<UserAutoLogin>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("user_auto_login")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.IsStaff).HasColumnName("is_staff");
            entity.Property(e => e.Key)
                .HasMaxLength(191)
                .HasColumnName("key");
            entity.Property(e => e.LastIp)
                .HasMaxLength(191)
                .HasColumnName("last_ip");
            entity.Property(e => e.LastLogin)
                .HasMaxLength(191)
                .HasColumnName("last_login");
            entity.Property(e => e.UserAgent)
                .HasMaxLength(191)
                .HasColumnName("user_agent");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        modelBuilder.Entity<UserMetum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("user_meta")
                .UseCollation("utf8mb4_unicode_ci");

            entity.HasIndex(e => e.ContactId, "user_meta_contact_id_fkey");

            entity.HasIndex(e => e.StaffId, "user_meta_staff_id_fkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClientId).HasColumnName("client_id");
            entity.Property(e => e.ContactId).HasColumnName("contact_id");
            entity.Property(e => e.MetaKey)
                .HasMaxLength(191)
                .HasColumnName("meta_key");
            entity.Property(e => e.MetaValue)
                .HasMaxLength(191)
                .HasColumnName("meta_value");
            entity.Property(e => e.StaffId).HasColumnName("staff_id");

            entity.HasOne(d => d.Contact).WithMany(p => p.UserMeta)
                .HasForeignKey(d => d.ContactId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user_meta_contact_id_fkey");

            entity.HasOne(d => d.Staff).WithMany(p => p.UserMeta)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user_meta_staff_id_fkey");
        });

        modelBuilder.Entity<Vault>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("vault")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Creator).HasColumnName("creator");
            entity.Property(e => e.CreatorName)
                .HasMaxLength(191)
                .HasColumnName("creator_name");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(3)")
                .HasColumnType("datetime(3)")
                .HasColumnName("date_created");
            entity.Property(e => e.Description)
                .HasMaxLength(191)
                .HasColumnName("description");
            entity.Property(e => e.LastUpdated)
                .HasMaxLength(191)
                .HasColumnName("last_updated");
            entity.Property(e => e.LastUpdatedFrom)
                .HasMaxLength(191)
                .HasColumnName("last_updated_from");
            entity.Property(e => e.Password)
                .HasMaxLength(191)
                .HasColumnName("password");
            entity.Property(e => e.Port).HasColumnName("port");
            entity.Property(e => e.ServerAddress)
                .HasMaxLength(191)
                .HasColumnName("server_address");
            entity.Property(e => e.ShareInProjects).HasColumnName("share_in_projects");
            entity.Property(e => e.Username)
                .HasMaxLength(191)
                .HasColumnName("username");
            entity.Property(e => e.Visibility).HasColumnName("visibility");
        });

        modelBuilder.Entity<ViewsTracking>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("views_tracking")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Date)
                .HasColumnType("datetime(3)")
                .HasColumnName("date");
            entity.Property(e => e.RelId).HasColumnName("rel_id");
            entity.Property(e => e.RelType)
                .HasMaxLength(191)
                .HasColumnName("rel_type");
            entity.Property(e => e.ViewIp)
                .HasMaxLength(191)
                .HasColumnName("view_ip");
        });

        modelBuilder.Entity<WebToLead>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("web_to_lead")
                .UseCollation("utf8mb4_unicode_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AllowDuplicate).HasColumnName("allow_duplicate");
            entity.Property(e => e.CreateTaskOnDuplicate).HasColumnName("create_task_on_duplicate");
            entity.Property(e => e.DateCreated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(3)")
                .HasColumnType("datetime(3)")
                .HasColumnName("date_created");
            entity.Property(e => e.FormData)
                .HasMaxLength(191)
                .HasColumnName("form_data");
            entity.Property(e => e.FormKey)
                .HasMaxLength(191)
                .HasColumnName("form_key");
            entity.Property(e => e.Language)
                .HasMaxLength(191)
                .HasColumnName("language");
            entity.Property(e => e.LeadNamePrefix)
                .HasMaxLength(191)
                .HasColumnName("lead_name_prefix");
            entity.Property(e => e.LeadSource).HasColumnName("lead_source");
            entity.Property(e => e.LeadStatus).HasColumnName("lead_status");
            entity.Property(e => e.MarkPublic).HasColumnName("mark_public");
            entity.Property(e => e.Name)
                .HasMaxLength(191)
                .HasColumnName("name");
            entity.Property(e => e.NotifyIds)
                .HasMaxLength(191)
                .HasColumnName("notify_ids");
            entity.Property(e => e.NotifyLeadImported).HasColumnName("notify_lead_imported");
            entity.Property(e => e.NotifyType)
                .HasMaxLength(191)
                .HasColumnName("notify_type");
            entity.Property(e => e.Recaptcha).HasColumnName("recaptcha");
            entity.Property(e => e.Responsible).HasColumnName("responsible");
            entity.Property(e => e.SubmitAction).HasColumnName("submit_action");
            entity.Property(e => e.SubmitBtnBgColor)
                .HasMaxLength(191)
                .HasColumnName("submit_btn_bg_color");
            entity.Property(e => e.SubmitBtnName)
                .HasMaxLength(191)
                .HasColumnName("submit_btn_name");
            entity.Property(e => e.SubmitBtnTextColor)
                .HasMaxLength(191)
                .HasColumnName("submit_btn_text_color");
            entity.Property(e => e.SubmitRedirectUrl)
                .HasMaxLength(191)
                .HasColumnName("submit_redirect_url");
            entity.Property(e => e.SuccessSubmitMsg)
                .HasMaxLength(191)
                .HasColumnName("success_submit_msg");
            entity.Property(e => e.TrackDuplicateField)
                .HasMaxLength(191)
                .HasColumnName("track_duplicate_field");
            entity.Property(e => e.TrackDuplicateFieldAnd)
                .HasMaxLength(191)
                .HasColumnName("track_duplicate_field_and");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
