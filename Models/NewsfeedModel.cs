using System.Dynamic;
using Newtonsoft.Json;
using Service.Core.Extensions;
using Service.Entities;
using Service.Framework;
using Service.Framework.Core.Extensions;
using Service.Framework.Helpers.Entities;
using Service.Helpers;
using Service.Models.Users;
using File = Service.Entities.File;


namespace Service.Models;

public class NewsfeedModel(MyInstance self, MyContext db) : MyModel(self)
{
  private const int post_likes_limit = 6;
  private const int post_comment_likes_limit = 6;
  private const int post_comments_limit = 6;
  private const int newsfeed_posts_limit = 10;
  private DepartmentsModel departments_model = self.model.departments_model();
  private StaffModel staff_model = self.model.staff_model();


  public bool pin_post(int id)
  {
    var post = db.NewsfeedPosts.FirstOrDefault(p => p.Id == id);
    if (post == null) return false;
    post.Pinned = true;
    post.DatePinned = DateTime.Now;

    return true;
  }

  public bool unpin_post(int id)
  {
    var post = db.NewsfeedPosts.FirstOrDefault(p => p.Id == id);
    if (post == null) return false;
    post.Pinned = false;
    post.DatePinned = null;

    return true;
  }

  public List<File> get_post_attachments(int id, bool images = false)
  {
    var query = db.Files.Where(f => f.RelId == id && f.RelType == "newsfeed_post");

    query = images
      ? query.Where(f => f.FileType.Contains("image"))
      : query.Where(f => !f.FileType.Contains("image"));

    return query.ToList();
  }

  public List<NewsfeedPostLike> get_post_likes(int id)
  {
    var rows = db.NewsfeedPostLikes
      .Where(l => l.PostId == id && l.UserId != staff_user_id)
      .OrderBy(l => l.DateLiked)
      .ToList();
    return rows;
  }

  public List<NewsfeedPostLike> load_likes_modal(int offset, int postid)
  {
    offset *= post_likes_limit;
    var rows = db.NewsfeedPostLikes
      .Where(l => l.PostId == postid)
      .OrderByDescending(l => l.DateLiked)
      .Skip(offset)
      .Take(post_likes_limit)
      .ToList();
    return rows;
  }

  public List<NewsfeedCommentLike> load_comment_likes_model(int offset, int commentid)
  {
    offset *= post_comment_likes_limit;
    var rows = db.NewsfeedCommentLikes
      .Where(l => l.CommentId == commentid)
      .OrderByDescending(l => l.DateLiked)
      .Skip(offset)
      .Take(post_comment_likes_limit)
      .ToList();
    return rows;
  }

  public List<NewsfeedPost> load_newsfeed(int offset, int? post_id = null)
  {
    offset *= newsfeed_posts_limit;
    var query = db.NewsfeedPosts.AsQueryable();

    if (!post_id.HasValue)
    {
      query = query.Where(p => p.Id == post_id);
    }
    else
    {
      query = query.Where(p => !p.Pinned);
      query = query.OrderByDescending(p => p.DateCreated)
        .Skip(offset)
        .Take(newsfeed_posts_limit);
    }

    return query.ToList();
  }

  public List<NewsfeedPost> get_pinned_posts()
  {
    var rows = db.NewsfeedPosts
      .Where(p => p.Pinned)
      .OrderBy(p => p.DatePinned)
      .ToList();
    return rows;
  }

  public NewsfeedPost? get_post(int id)
  {
    return db.NewsfeedPosts.FirstOrDefault(p => p.Id == id);
  }

  public List<NewsfeedPostComment> get_comment()
  {
    var rows = db.NewsfeedPostComments.ToList();
    return rows;
  }

  public NewsfeedPostComment get_comment(int id)
  {
    var row = db.NewsfeedPostComments.FirstOrDefault(c => c.Id == id);
    return row;
  }

  public int add(NewsfeedPost post)
  {
    post.DateCreated = DateTime.Now;
    post.Content = post.Content.nl2br();
    post.Creator = staff_user_id;


    // if (!string.IsNullOrEmpty(post.Visibility))
    //   post.Visibility = "all";
    // else
    //   post.Visibility = string.Join(':', post.Visibility);


    var result = db.NewsfeedPosts.Add(post);
    var postid = result.Entity.Id;

    var staff = staff_model.get(x => x.Active!.Value && x.IsNotStaff == 0);
    var notifiedUsers = new List<int>();
    staff.ForEach(member =>
    {
      var staff_deparments = departments_model.get_staff_departments(member.Id);
      var visibility = post.Visibility.Split(":").ToList();
      visibility.ForEach(i =>
      {
        // if (!staff_deparments.Contains(Convert.ToInt32(i))) return;
        // Allow admin to view all posts
        if (member.Id.is_admin()) return;
        // continue 2;
      });
      if (post.Creator == member.Id) return;
      var notified = self.helper.add_notification(new Notification
      {
        Description = "not_published_new_post",
        ToUserId = member.Id,
        Link = "#postid=" + postid
      });

      if (notified) notifiedUsers.Add(member.Id);
    });
    self.helper.pusher_trigger_notification(notifiedUsers);
    return postid;
  }

  public async Task<bool> like_post(int id)
  {
    if (user_liked_post(id)) return true;

    var like = new NewsfeedPostLike
    {
      PostId = id,
      UserId = staff_user_id,
      DateLiked = DateTime.Now
    };

    db.NewsfeedPostLikes.Add(like);


    // Notify post creator if necessary...

    return true;
  }

  public bool unlike_post(int id)
  {
    var like = db.NewsfeedPostLikes
      .FirstOrDefault(l => l.PostId == id && l.UserId == staff_user_id);

    if (like == null) return false;
    db.NewsfeedPostLikes.Remove(like);

    return true;
  }

  public bool unlike_comment(int id, int postid)
  {
    var affected_rows = db.NewsfeedCommentLikes
      .Where(l =>
        l.CommentId == id &&
        l.PostId == postid &&
        l.UserId == staff_user_id
      )
      .Delete();

    return affected_rows > 0;
  }

  public bool user_liked_post(int id)
  {
    return db.NewsfeedPostLikes
      .Any(l => l.PostId == id && l.UserId == staff_user_id);
  }

  public bool user_liked_comment(int id)
  {
    return db.NewsfeedCommentLikes
      .Any(l => l.CommentId == id && l.UserId == staff_user_id);
  }

  public int add_comment(NewsfeedPostComment comment)
  {
    comment.DateCreated = DateTime.Now;
    comment.UserId = staff_user_id;
    comment.Content = comment.Content?.nl2br();
    var result = db.NewsfeedPostComments.Add(comment);
    var insert_id = result.Entity.Id;
    if (!result.IsAdded()) return 0;
    // var post = this.get_post(post['postid']);
    dynamic post = new ExpandoObject();
    if (post.creator == self.helper.get_staff_user_id()) return insert_id;
    var notified = self.helper.add_notification(new Notification
    {
      Description = "not_commented_your_post",
      ToUserId = post.creator,
      Link = "#postid=" + comment.PostId,
      AdditionalData = JsonConvert.SerializeObject(new[]
      {
        self.helper.get_staff_full_name(self.helper.get_staff_user_id()),
        $"{post.content}"[..50]
      })
    });
    if (notified)
      self.helper.pusher_trigger_notification(new List<int> { post.creator });
    return insert_id;
  }

  public bool like_comment(int id, int postid)
  {
    if (user_liked_comment(id)) return true;

    var like = new NewsfeedCommentLike
    {
      CommentId = id,
      PostId = postid,
      UserId = staff_user_id,
      DateLiked = today()
    };

    db.NewsfeedCommentLikes.Add(like);


    // Notify comment creator if necessary...

    return true;
  }

  public bool remove_post_comment(int id, int postid)
  {
    var comment = db.NewsfeedPostComments
      .FirstOrDefault(c => c.Id == id && c.PostId == postid && c.UserId == staff_user_id);

    if (comment == null) return false;
    db.NewsfeedPostComments.Remove(comment);

    return true;
  }

  public bool delete_post(int postid)
  {
    var post = db.NewsfeedPosts
      .FirstOrDefault(p => p.Id == postid && (p.Creator == staff_user_id || is_admin));

    if (post == null) return false;
    db.NewsfeedPosts
      .Where(p => p.Id == postid && (p.Creator == staff_user_id || is_admin))
      .Delete();

    db.NewsfeedPostLikes.RemoveRange(
      db.NewsfeedPostLikes.Where(l => l.PostId == postid));
    db.NewsfeedPostComments.RemoveRange(
      db.NewsfeedPostComments.Where(c => c.PostId == postid));
    db.NewsfeedCommentLikes.RemoveRange(
      db.NewsfeedCommentLikes.Where(l => l.PostId == postid));

    db.SaveChanges();

    // Delete associated files and directories if necessary...

    return true;
  }

  public List<NewsfeedPostComment> get_post_comments(int postid, int offset)
  {
    offset *= post_comments_limit;
    var rows = db.NewsfeedPostComments
      .Where(c => c.PostId == postid)
      .OrderByDescending(c => c.DateCreated)
      .Skip(offset)
      .Take(post_comments_limit)
      .ToList();
    return rows;
  }
}
