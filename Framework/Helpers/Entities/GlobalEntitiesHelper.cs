using System.ComponentModel;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Service.Framework.Helpers.Entities;

public static class EntitiesExtensions
{
  public static bool is_reference_in_table<TEntity>(this DbContext context, string field, int id) where TEntity : class
  {
    var entityType = context.Model.FindEntityType(typeof(TEntity)) ?? throw new ArgumentException("Invalid table entity");
    var parameter = Expression.Parameter(typeof(TEntity), "e");
    var property = Expression.PropertyOrField(parameter, field);
    var constant = Expression.Constant(id);
    var equalExpression = Expression.Equal(property, constant);
    var lambda = Expression.Lambda<Func<TEntity, bool>>(equalExpression, parameter);
    return context.Set<TEntity>().Any(lambda);
  }

  public static bool IsAdded(this EntityEntry entry)
  {
    return entry.State == EntityState.Added;
  }

  public static bool IsUpdated(this EntityEntry entry)
  {
    return entry.State == EntityState.Modified;
  }

  public static bool IsModified(this EntityEntry entry)
  {
    return entry.State == EntityState.Modified;
  }

  public static bool IsDeleted(this EntityEntry entry)
  {
    return entry.State == EntityState.Deleted;
  }


  public static List<string> list_fields<TEntity>(this DbContext context) where TEntity : class
  {
    var field = "id";
    var id = 1;
    var output = new List<string>();
    // Get the entity type and ensure it's valid
    var entityType = context.Model.FindEntityType(typeof(TEntity)) ?? throw new ArgumentException("Invalid table entity");
    // Create a parameter expression for the entity
    var parameter = Expression.Parameter(typeof(TEntity), "e");
    // Build a property or field expression based on the provided 'field'
    var property = Expression.PropertyOrField(parameter, field);
    // Create an equality expression 'e.field == id'
    var constant = Expression.Constant(id);
    var equalExpression = Expression.Equal(property, constant);

    // Create a lambda expression for filtering the entity by 'field == id'
    var lambda = Expression.Lambda<Func<TEntity, bool>>(equalExpression, parameter);

    // Query the first matching row
    var row = context.Set<TEntity>().FirstOrDefault(lambda);

    if (row == null) return output; // Return empty list if no row is found

    // Use LINQ to select property names and convert to a list
    output = TypeDescriptor.GetProperties(row)
      .Cast<PropertyDescriptor>() // Cast to PropertyDescriptor
      .Select(desc => desc.Name) // Select the property name
      .ToList(); // Convert to a list

    return output;
  }

  public static int? ExtractIdFromCondition<T>(this DbContext db, Expression<Func<T, bool>> condition) where T : class
  {
    if (condition.Body is not BinaryExpression { NodeType: ExpressionType.Equal } binaryExpression)
      return null;
    if (binaryExpression.Left is not MemberExpression memberExpression ||
        memberExpression.Member.Name != "Id")
      return null;
    if (memberExpression.Expression is not ParameterExpression parameterExpression ||
        parameterExpression.Type != typeof(T))
      return null;
    if (binaryExpression.Right is ConstantExpression constantExpression)
      return constantExpression.Value as int?;
    return null;
  }
}
