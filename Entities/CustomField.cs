namespace Service.Entities;

public partial class CustomField
{
    public int Id { get; set; }

    public string FieldTo { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public int Required { get; set; }

    public string Type { get; set; } = null!;

    public string? Options { get; set; }

    public int DisplayInline { get; set; }

    public int? FieldOrder { get; set; }

    public bool Active { get; set; }

    public bool ShowOnPdf { get; set; }

    public bool ShowOnTicketForm { get; set; }

    public bool OnlyAdmin { get; set; }

    public bool ShowOnTable { get; set; }

    public bool ShowOnClientPortal { get; set; }

    public int DisalowClientToEdit { get; set; }

    public int BsColumn { get; set; }

    public string? DefaultValue { get; set; }

    public virtual ICollection<CustomFieldsValue> CustomFieldsValues { get; set; } = new List<CustomFieldsValue>();
}
