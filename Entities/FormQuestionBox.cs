namespace Service.Entities;

public partial class FormQuestionBox
{
    public int Id { get; set; }

    public string Type { get; set; } = null!;

    public int QuestionId { get; set; }
}
