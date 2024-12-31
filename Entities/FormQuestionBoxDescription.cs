namespace Service.Entities;

public partial class FormQuestionBoxDescription
{
    public int Questionboxdescriptionid { get; set; }

    public string Description { get; set; } = null!;

    public string BoxId { get; set; } = null!;

    public int QuestionId { get; set; }
}
