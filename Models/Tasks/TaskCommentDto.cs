using Service.Entities;
using File = Service.Entities.File;

namespace Service.Models.Tasks;

public class TaskCommentDto : TaskComment
{
  public List<File> attachments = new();
}
