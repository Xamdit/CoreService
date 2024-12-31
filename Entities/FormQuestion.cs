namespace Service.Entities;

public partial class FormQuestion
{
    public int Id { get; set; }

    public int RelId { get; set; }

    public string RelType { get; set; } = null!;

    public string Question { get; set; } = null!;

    public int Required { get; set; }

    public int QuestionOrder { get; set; }
}
