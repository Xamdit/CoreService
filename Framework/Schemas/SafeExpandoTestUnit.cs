// <copyright file="SafeExpandoTestUnit.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Service.Framework.Schemas;

public class SafeExpandoTestUnit
{
  [Test]
  public void Test()
  {
    var safeExpando = new SafeExpando();
    dynamic expando = safeExpando;
    expando.Name = "Test"; // Set a property
    Console.WriteLine(expando.Name); // Output: Test
    Console.WriteLine(expando.NonExistentProperty); // Output: null instead of throwing an exception
  }

  // [Test]
  // public void TryGetValue_NonexistentKey_ReturnsNull()
  // {
  //   var expando = new SafeExpando();
  //   object value;
  //
  //   Assert.That(expando.TryGetValue("unknown", out value), Is.False);
  //   Assert.That(value, Is.Null);
  // }
  //
  // [Test]
  // public void IndexedGetter_NonexistentKey_ReturnsNull()
  // {
  //   var expando = new SafeExpando();
  //   Assert.That(expando["missing"], Is.Null);
  // }
}
