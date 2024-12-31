namespace Service.Entities;

public partial class CustomFieldsValue
{
    public int Id { get; set; }

    public int RelId { get; set; }

    public int? FieldId { get; set; }

    public string FieldTo { get; set; } = null!;

    public string Value { get; set; } = null!;

    public virtual CustomField? Field { get; set; }
}
