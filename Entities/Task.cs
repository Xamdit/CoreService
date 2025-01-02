namespace Service.Entities;

public partial class Task
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public int? Priority { get; set; }

    public DateTime DateCreated { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? DueDate { get; set; }

    public DateTime? DateFinished { get; set; }

    public int AddedFrom { get; set; }

    public bool IsAddedFromContact { get; set; }

    public int Status { get; set; }

    public string RecurringType { get; set; } = null!;

    public string RepeatEvery { get; set; } = null!;

    public int Recurring { get; set; }

    public int? IsRecurringFrom { get; set; }

    public int Cycles { get; set; }

    public int TotalCycles { get; set; }

    public int CustomRecurring { get; set; }

    public string? LastRecurringDate { get; set; }

    public int? RelId { get; set; }

    public string RelType { get; set; } = null!;

    public bool IsPublic { get; set; }

    public int Billable { get; set; }

    public int Billed { get; set; }

    public int? InvoiceId { get; set; }

    public double HourlyRate { get; set; }

    public int? Milestone { get; set; }

    public int? KanbanOrder { get; set; }

    public int MilestoneOrder { get; set; }

    public bool VisibleToClient { get; set; }

    public int DeadlineNotified { get; set; }

    public virtual Invoice? Invoice { get; set; }

    public virtual ICollection<TaskAssigned> TaskAssigneds { get; set; } = new List<TaskAssigned>();

    public virtual ICollection<TaskChecklistItem> TaskChecklistItems { get; set; } = new List<TaskChecklistItem>();

    public virtual ICollection<TaskComment> TaskComments { get; set; } = new List<TaskComment>();

    public virtual ICollection<TaskFollower> TaskFollowers { get; set; } = new List<TaskFollower>();

    public virtual ICollection<TasksTimer> TasksTimers { get; set; } = new List<TasksTimer>();
}
