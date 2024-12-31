namespace Service.Entities;

public partial class FormResult
{
    public int Id { get; set; }

    public int BoxId { get; set; }

    public int? Boxdescriptionid { get; set; }

    public int RelId { get; set; }

    public string RelType { get; set; } = null!;

    public int QuestionId { get; set; }

    public string? Answer { get; set; }

    public int ResultsetId { get; set; }
}
