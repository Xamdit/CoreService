namespace Service.Entities;

public partial class SharedCustomerFile
{
    public int Id { get; set; }

    public int FileId { get; set; }

    public int? ContactId { get; set; }

    public virtual Contact? Contact { get; set; }

    public virtual File File { get; set; } = null!;
}
