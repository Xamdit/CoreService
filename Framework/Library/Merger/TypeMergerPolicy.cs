using System.Linq.Expressions;

namespace Service.Framework.Library.Merger;

// </summary>
public class TypeMergerPolicy
{
  public TypeMergerPolicy()
  {
    IgnoredProperties = new List<Tuple<string, string>>();
    UseProperties = new List<Tuple<string, string>>();
  }

  internal IList<Tuple<string, string>> IgnoredProperties { get; }

  internal IList<Tuple<string, string>> UseProperties { get; }

  /// <summary>
  ///
  /// </summary>
  /// <param name="ignoreProperty">The property of the object to be ignored as a Func.</param>
  /// <returns></returns>
  public TypeMergerPolicy Ignore(Expression<Func<object>> ignoreProperty)
  {
    IgnoredProperties.Add(GetObjectTypeAndProperty(ignoreProperty));
    return this;
  }

  /// <summary>
  ///
  /// </summary>
  /// <param name="instance">The object instance.</param>
  /// <returns></returns>
  public TypeMergerPolicy Ignore<T>(T instance, string ignoreProperty)
  {
    IgnoredProperties.Add(new Tuple<string, string>(instance.GetType().Name, ignoreProperty));
    return this;
  }

  /// <summary>
  ///
  /// </summary>
  /// <param name="useProperty"></param>
  /// <returns></returns>
  public TypeMergerPolicy Use(Expression<Func<object>> useProperty)
  {
    UseProperties.Add(GetObjectTypeAndProperty(useProperty));
    return this;
  }

  /// <summary>
  ///
  /// </summary>
  /// property value from 'values1' will be used.
  /// <returns>New object containing properties from both objects.</returns>
  public object Merge(object values1, object values2)
  {
    return TypeMerger.Merge(values1, values2, this);
  }

  /// <summary>
  ///
  /// </summary>
  /// <param name="property">The property to inspect as a Func Expression.</param>
  private Tuple<string, string> GetObjectTypeAndProperty(Expression<Func<object>> property)
  {
    var objType = string.Empty;
    var propName = string.Empty;
    try
    {
      switch (property.Body)
      {
        case MemberExpression expression:
          objType = expression.Expression.Type.Name;
          propName = expression.Member.Name;
          break;
        case UnaryExpression expression:
          objType = ((MemberExpression)expression.Operand).Expression.Type.Name;
          propName = ((MemberExpression)expression.Operand).Member.Name;
          break;
        default:
          throw new Exception("Expression type unknown.");
      }
    }
    catch (Exception ex)
    {
      throw new Exception("Error in TypeMergePolicy.GetObjectTypeAndProperty.", ex);
    }

    return new Tuple<string, string>(objType, propName);
  }
}
