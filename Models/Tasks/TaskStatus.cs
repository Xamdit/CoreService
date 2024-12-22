namespace Service.Models.Tasks;

public static class TaskStatus
{
  public const int STATUS_NOT_STARTED = 1;

  public const int STATUS_AWAITING_FEEDBACK = 2;

  public const int STATUS_TESTING = 3;

  public const int STATUS_IN_PROGRESS = 4;

  public const int STATUS_COMPLETE = 5;
}
