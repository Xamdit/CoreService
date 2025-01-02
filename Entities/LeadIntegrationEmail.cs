namespace Service.Entities;

public partial class LeadIntegrationEmail
{
    public int Id { get; set; }

    public string? Subject { get; set; }

    public string? Body { get; set; }

    public DateTime DateCreated { get; set; }

    public int? LeadId { get; set; }

    public int Emailid { get; set; }

    public virtual Lead? Lead { get; set; }
}
