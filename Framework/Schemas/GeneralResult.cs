using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Service.Framework.Schemas;

public class BaseResult : SafeExpando
{
  public bool Success { get; set; }
  public string Message { get; set; }
  public bool Referenced { get; set; }
  public bool IsDefault { get; set; }
  public bool HasTransactionsCurrency { get; set; }

  protected BaseResult(bool success = false, string message = "", bool referenced = false, bool isDefault = false)
  {
    Success = success;
    Message = message;
    Referenced = referenced;
    IsDefault = isDefault;
  }
}

public class CreatedResult<T> : BaseResult
  where T : class
{
  public CreatedResult(EntityEntry<T> result)
  {
    Data = result.Entity;
  }

  public CreatedResult()
  {
  }

  public int InsertId { get; set; }
  public T Data { get; set; }

  public CreatedResult<T> MakeResult(EntityEntry<T> entry)
  {
    // InsertId = entry.Entity.Id;
    Data = entry.Entity;
    return this;
  }
}

public class ReadResult : BaseResult
{
}

public class UpdatedResult : BaseResult
{
  public int AffectedRow { get; set; }

  public UpdatedResult(bool success = false, string message = "", bool referenced = false,
    bool isDefault = false)
    : base(
      success, message, referenced, isDefault)
  {
  }

  public static UpdatedResult Compile(bool success = false, string message = "", bool referenced = false,
    bool isDefault = false)
  {
    return new UpdatedResult(success, message, referenced, isDefault);
  }
}

public class DeletedResult : BaseResult
{
  public DeletedResult(bool success = false, string message = "", bool referenced = false,
    bool isDefault = false)
    : base(
      success, message, referenced, isDefault)
  {
  }

  public static DeletedResult Compile(bool success = false, string message = "", bool referenced = false,
    bool isDefault = false)
  {
    return new DeletedResult(success, message, referenced, isDefault);
  }
}
