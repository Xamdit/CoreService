namespace Service.Entities;

public partial class ProjectDiscussion
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    public string Subject { get; set; } = null!;

    public string Description { get; set; } = null!;

    public bool ShowToCustomer { get; set; }

    public DateTime DateCreated { get; set; }

    public DateTime? LastActivity { get; set; }

    public int StaffId { get; set; }

    public int ContactId { get; set; }

    public virtual Contact Contact { get; set; } = null!;

    public virtual Project Project { get; set; } = null!;

    public virtual ICollection<ProjectDiscussionComment> ProjectDiscussionComments { get; set; } = new List<ProjectDiscussionComment>();

    public virtual Staff Staff { get; set; } = null!;
}
