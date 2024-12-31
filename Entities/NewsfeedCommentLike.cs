namespace Service.Entities;

public partial class NewsfeedCommentLike
{
    public int Id { get; set; }

    public int PostId { get; set; }

    public int CommentId { get; set; }

    public int UserId { get; set; }

    public string DateLiked { get; set; } = null!;

    public virtual NewsfeedPostComment Comment { get; set; } = null!;

    public virtual NewsfeedPost Post { get; set; } = null!;
}
