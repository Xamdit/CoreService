namespace Service.Entities;

public partial class Notification
{
    public int Id { get; set; }

    public bool IsRead { get; set; }

    public int IsReadInline { get; set; }

    public DateTime Date { get; set; }

    public string Description { get; set; } = null!;

    public int FromUserId { get; set; }

    public int? FromClientId { get; set; }

    public string FromFullname { get; set; } = null!;

    public int ToUserId { get; set; }

    public bool? FromCompany { get; set; }

    public string? Link { get; set; }

    public string? AdditionalData { get; set; }

    public virtual Client? FromClient { get; set; }
}
