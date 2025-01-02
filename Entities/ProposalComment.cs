namespace Service.Entities;

public partial class ProposalComment
{
    public int Id { get; set; }

    public string? Content { get; set; }

    public int ProposalId { get; set; }

    public int StaffId { get; set; }

    public DateTime DateCreated { get; set; }

    public virtual Proposal Proposal { get; set; } = null!;

    public virtual Staff Staff { get; set; } = null!;
}
