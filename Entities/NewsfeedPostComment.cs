namespace Service.Entities;

public partial class NewsfeedPostComment
{
    public int Id { get; set; }

    public string? Content { get; set; }

    public int UserId { get; set; }

    public int PostId { get; set; }

    public DateTime DateCreated { get; set; }

    public virtual ICollection<NewsfeedCommentLike> NewsfeedCommentLikes { get; set; } = new List<NewsfeedCommentLike>();

    public virtual NewsfeedPost Post { get; set; } = null!;
}
