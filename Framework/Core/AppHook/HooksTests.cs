namespace Service.Framework.Core.AppHook;

public class HooksTests
{
  private readonly Hooks _hooks = new();

  [Test]
  public void AddFilter_ShouldAddFilterSuccessfully()
  {
    // Arrange
    var tag = "testFilter";
    Func<object, object> filterFunction = obj => obj.ToString() + " modified";

    // Act
    var result = _hooks.AddFilter(tag, filterFunction);

    // Assert
    Assert.Equals(result, true);
    Assert.Equals(_hooks.HasFilter(tag, filterFunction), true);
  }

  [Test]
  public void RemoveFilter_ShouldRemoveFilterSuccessfully()
  {
    // Arrange
    var tag = "testFilter";
    Func<object, object> filterFunction = obj => obj.ToString() + " modified";
    _hooks.AddFilter(tag, filterFunction);

    // Act
    var result = _hooks.RemoveFilter(tag, filterFunction);

    // Assert
    Assert.Equals(result, true);
    Assert.Equals(_hooks.HasFilter(tag, filterFunction), false);
  }

  [Test]
  public void ApplyFilters_ShouldApplyFilterAndModifyValue()
  {
    // Arrange
    var tag = "testFilter";
    _hooks.AddFilter(tag, obj => obj.ToString() + " modified");

    // Act
    var result = _hooks.apply_filters(tag, "initial value");

    // Assert
    Assert.Equals("initial value modified", result);
  }

  [Test]
  public void AddAction_ShouldTriggerActionSuccessfully()
  {
    // Arrange
    var tag = "testAction";
    var actionExecuted = false;
    _hooks.AddAction(tag, () => actionExecuted = true);

    // Act
    _hooks.do_action(tag);

    // Assert
    Assert.Equals(actionExecuted, true);
  }

  [Test]
  public void DidAction_ShouldReturnCorrectActionCount()
  {
    // Arrange
    var tag = "testAction";
    _hooks.AddAction(tag, () => { });

    // Act
    _hooks.do_action(tag);
    _hooks.do_action(tag);

    var actionCount = _hooks.DidAction(tag);

    // Assert
    Assert.Equals(2, actionCount);
  }

  [Test]
  public void RemoveAction_ShouldRemoveActionSuccessfully()
  {
    // Arrange
    var tag = "testAction";
    var actionExecuted = false;
    Action testAction = () => actionExecuted = true;
    _hooks.AddAction(tag, testAction);

    // Act
    var removeResult = _hooks.RemoveAction(tag, testAction);
    _hooks.do_action(tag);

    // Assert
    Assert.Equals(removeResult, true);
    Assert.Equals(actionExecuted, false);
  }

  [Test]
  public void RemoveAllFilters_ShouldClearAllFilters()
  {
    // Arrange
    var tag = "testFilter";
    _hooks.AddFilter(tag, obj => obj.ToString() + " modified");

    // Act
    _hooks.RemoveAllFilters(tag);

    // Assert
    Assert.Equals(_hooks.HasFilter(tag), false);
  }

  [Test]
  public void CurrentFilter_ShouldReturnCorrectCurrentFilter()
  {
    // Arrange
    var tag = "testFilter";
    _hooks.AddFilter(tag, obj => obj.ToString());

    // Act
    _hooks.apply_filters(tag, "testValue");

    // Assert
    Assert.Equals(tag, _hooks.CurrentFilter());
  }

  [Test]
  public void TestHook()
  {
    var tag = "numbers";
    var input = new[] { 1, 2, 3, 4, 5 };
    var result1 = _hooks.apply_filters(tag, input);
    Console.WriteLine(string.Join(", ", result1["Value1"])); // Prints: 1, 2, 3, 4, 5

    var hello = "Hello";
    var world = "World";
    var result2 = _hooks.apply_filters(tag, hello, world);

// Access dynamically using keys
    Console.WriteLine(result2["Value1"]); // Prints: Hello
    Console.WriteLine(result2["Value2"]); // Prints: World

// To dynamic for cleaner syntax
    var dynamicResult = result2.ToDynamic();
    Console.WriteLine(dynamicResult.Value1); // Prints: Hello
    Console.WriteLine(dynamicResult.Value2); // Prints: World
  }
}
