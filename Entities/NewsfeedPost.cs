namespace Service.Entities;

public partial class NewsfeedPost
{
    public int Id { get; set; }

    public int Creator { get; set; }

    public DateTime DateCreated { get; set; }

    public string Visibility { get; set; } = null!;

    public string Content { get; set; } = null!;

    public bool Pinned { get; set; }

    public DateTime? DatePinned { get; set; }

    public virtual ICollection<NewsfeedCommentLike> NewsfeedCommentLikes { get; set; } = new List<NewsfeedCommentLike>();

    public virtual ICollection<NewsfeedPostComment> NewsfeedPostComments { get; set; } = new List<NewsfeedPostComment>();

    public virtual ICollection<NewsfeedPostLike> NewsfeedPostLikes { get; set; } = new List<NewsfeedPostLike>();
}
