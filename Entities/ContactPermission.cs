namespace Service.Entities;

public partial class ContactPermission
{
    public int Id { get; set; }

    public int PermissionId { get; set; }

    public int UserId { get; set; }
}
