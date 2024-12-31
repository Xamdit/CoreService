namespace Service.Entities;

public partial class NewsfeedPostLike
{
    public int Id { get; set; }

    public int PostId { get; set; }

    public int UserId { get; set; }

    public DateTime DateLiked { get; set; }

    public virtual NewsfeedPost Post { get; set; } = null!;
}
