namespace Service.Entities;

public partial class ContractRenewal
{
    public int Id { get; set; }

    public int? ContractId { get; set; }

    public DateTime OldStartDate { get; set; }

    public DateTime NewStartDate { get; set; }

    public DateTime? OldEndDate { get; set; }

    public DateTime? NewEndDate { get; set; }

    public bool OldValue { get; set; }

    public bool NewValue { get; set; }

    public DateTime DateRenewed { get; set; }

    public string RenewedBy { get; set; } = null!;

    public int RenewedByStaffId { get; set; }

    public int? IsOnOldExpiryNotified { get; set; }

    public virtual Contract? Contract { get; set; }

    public virtual Staff RenewedByStaff { get; set; } = null!;
}
