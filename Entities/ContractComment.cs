namespace Service.Entities;

public partial class ContractComment
{
    public int Id { get; set; }

    public string? Content { get; set; }

    public int? ContractId { get; set; }

    public int StaffId { get; set; }

    public DateTime DateCreated { get; set; }

    public virtual Contract? Contract { get; set; }

    public virtual Staff Staff { get; set; } = null!;
}
