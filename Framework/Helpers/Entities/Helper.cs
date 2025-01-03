using System.Linq.Expressions;

namespace Service.Framework.Helpers.Entities;

public static class EntitiesHelper
{
  public static Expression<Func<T, bool>> CreateCondition<T>(Expression<Func<T, bool>> condition = default)
  {
    return condition;
  }

  public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
  {
    var parameter = Expression.Parameter(typeof(T), "x");

    // Replace parameters in the second expression with the parameter of the first
    var body = Expression.AndAlso(
      Expression.Invoke(first, parameter),
      Expression.Invoke(second, parameter)
    );

    return Expression.Lambda<Func<T, bool>>(body, parameter);
  }

  public static Expression<Func<T, bool>> Or<T>(
    this Expression<Func<T, bool>> first,
    Expression<Func<T, bool>> second)
  {
    var parameter = Expression.Parameter(typeof(T), "x");

    // Replace parameters in the second expression with the parameter of the first
    var body = Expression.OrElse(
      Expression.Invoke(first, parameter),
      Expression.Invoke(second, parameter)
    );

    return Expression.Lambda<Func<T, bool>>(body, parameter);
  }
}
