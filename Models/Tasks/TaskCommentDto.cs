using Global.Entities;

namespace Service.Models.Tasks;

public class TaskCommentDto : TaskComment
{
  public List<Global.Entities.File> attachments = new();
}
