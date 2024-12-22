using Global.Entities;
using Service.Framework.Core.Engine;

namespace Service.Helpers.Tags;

public static class TagsHelper
{
  private static AppTags _appTags = new();
  private static AppObjectCache _appObjectCache = new();

  /// <summary>
  /// Function that adds and edits tags based on passed arguments
  /// </summary>
  /// <param name="tags">Tags</param>
  /// <param name="relId">Relationship ID</param>
  /// <param name="relType">Relationship type</param>
  /// <returns>Boolean</returns>
  public static bool handle_tags_save(this HelperBase helperBase, List<Taggable> tags, object relId, string relType)
  {
    var (self, db) = getInstance();
    return db._call_tags_method<bool>("save", tags, relId, relType);
  }

  /// <summary>
  /// Get tag from database by name
  /// </summary>
  /// <param name="name">Tag name</param>
  /// <returns>Tag object</returns>
  public static object get_tag_by_name(this MyContext db, string name)
  {
    return db._call_tags_method<object>("get", name);
  }

  /// <summary>
  /// Function that returns all tags used in the app
  /// </summary>
  /// <returns>Array of tags</returns>
  public static List<object> get_tags(this MyContext db)
  {
    return db._call_tags_method<List<object>>("all");
  }

  /// <summary>
  /// Array of available tags without the keys
  /// </summary>
  /// <returns>Array of tags</returns>
  public static List<object> get_tags_clean(this MyContext db)
  {
    return db._call_tags_method<List<object>>("flat");
  }

  /// <summary>
  /// Get all tag ids
  /// </summary>
  /// <returns>Array of tag ids</returns>
  public static List<int> get_tags_ids(this MyContext db)
  {
    return db._call_tags_method<List<int>>("ids");
  }

  /// <summary>
  /// Function that parses all the tags and returns array with the names
  /// </summary>
  /// <param name="relId">Relationship ID</param>
  /// <param name="relType">Relationship type</param>
  /// <returns>Array of tag names</returns>
  public static List<Taggable> get_tags_in(this MyContext db, int relId, string relType)
  {
    return db._call_tags_method<List<Taggable>>("relation", relId, relType);
  }

  /// <summary>
  /// Helper function to call AppTags method
  /// </summary>
  /// <param name="method">Method to call</param>
  /// <param name="params">Method parameters</param>
  /// <returns>Method result</returns>
  private static T _call_tags_method<T>(this MyContext db, string method, params object[] parameters)
  {
    if (_appTags == null) throw new Exception("AppTags library is not loaded");

    var methodInfo = _appTags.GetType().GetMethod(method);
    if (methodInfo == null) throw new Exception($"Method '{method}' not found in AppTags");

    return (T)methodInfo.Invoke(_appTags, parameters);
  }

  /// <summary>
  /// Comma-separated tags for input
  /// </summary>
  /// <param name="tagNames">Array of tag names</param>
  /// <returns>Comma-separated string of tags</returns>
  public static string prep_tags_input(this HelperBase helper, List<string> tagNames)
  {
    var filteredTags = tagNames.Where(value => !string.IsNullOrEmpty(value)).ToList();
    return string.Join(",", filteredTags);
  }

  /// <summary>
  /// Function will render tags as HTML version to show to the user
  /// </summary>
  /// <param name="tags">Tags</param>
  /// <returns>HTML string</returns>
  public static string render_tags(this MyContext db, object tags)
  {
    var tagsHtml = string.Empty;
    List<string> tagList;

    if (!(tags is List<string>))
      tagList = string.IsNullOrEmpty(tags?.ToString()) ? new List<string>() : tags.ToString().Split(',').ToList();
    else
      tagList = (List<string>)tags;

    tagList = tagList.Where(value => !string.IsNullOrEmpty(value)).ToList();

    if (tagList.Count <= 0) return tagsHtml;
    tagsHtml += "<div class='tags-labels'>";
    var i = 0;
    var len = tagList.Count;

    foreach (var tag in tagList)
    {
      var tagId = 0;
      var tagRow = _appObjectCache.get<object>($"tag-id-by-name-{tag}");

      if (tagRow == null)
      {
        tagRow = db.get_tag_by_name(tag);

        if (tagRow != null)
          _appObjectCache.add($"tag-id-by-name-{tag}", tagRow);
      }

      if (tagRow != null) tagId = tagRow is int row ? row : (int)(tagRow as dynamic)?.id;

      tagsHtml += $"<span class='label label-tag tag-id-{tagId}'><span class='tag'>{tag}</span><span class='hide'>{(i != len - 1 ? ", " : string.Empty)}</span></span>";
      i++;
    }

    tagsHtml += "</div>";

    return tagsHtml;
  }
}

// Placeholder classes for AppTags and AppObjectCache used in the original PHP code
public class AppTags
{
  public static void save(string tags, object relId, string relType)
  {
  }

  public static object get(string name)
  {
    return null;
  }

  public static List<object> all()
  {
    return new List<object>();
  }

  public static List<object> flat()
  {
    return new List<object>();
  }

  public static List<int> ids()
  {
    return new List<int>();
  }

  public static List<string> relation(string relId, string relType)
  {
    return new List<string>();
  }
}
