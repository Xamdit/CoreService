using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;

namespace Service.Framework.Library.Merger;

public static class TypeMerger
{
  private static AssemblyBuilder? asmBuilder;
  private static ModuleBuilder? modBuilder;
  private static TypeMergerPolicy? typeMergerPolicy;
  private static readonly IDictionary<string, Type> AnonymousTypes = new Dictionary<string, Type>();
  private static readonly object SyncLock = new();

  public static object Merge(object values1, object values2)
  {
    return Merge(values1, values2, null);
  }

  internal static object Merge(object values1, object values2, TypeMergerPolicy? policy)
  {
    lock (SyncLock)
    {
      typeMergerPolicy = policy;
      var name = string.Format(
        "{0}_{1}",
        values1.GetType(),
        values2.GetType());
      if (typeMergerPolicy != null)
        name += "_" + string.Join(
          ",",
          typeMergerPolicy.IgnoredProperties.Select(x => string.Format("{0}_{1}", x.Item1, x.Item2)));

      var result = CreateInstance(name, values1, values2, out name);
      if (result != null)
      {
        typeMergerPolicy = null;
        return result;
      }

      var pdc = GetProperties(values1, values2);
      InitializeAssembly();
      var newType = CreateType(name, pdc);
      AnonymousTypes.Add(name, newType);
      result = CreateInstance(name, values1, values2, out name);
      typeMergerPolicy = null;
      return result;
    }
  }

  public static TypeMergerPolicy Ignore(Expression<Func<object>> ignoreProperty)
  {
    return new TypeMergerPolicy().Ignore(ignoreProperty);
  }

  public static TypeMergerPolicy Ignore<T>(T instance, string ignoreProperty)
  {
    return new TypeMergerPolicy().Ignore(instance, ignoreProperty);
  }

  public static TypeMergerPolicy Use(Expression<Func<object>> useProperty)
  {
    return new TypeMergerPolicy().Use(useProperty);
  }

  private static object CreateInstance(string name, object values1, object values2, out string keyName)
  {
    object newValues = null;
    keyName = name;
    if (AnonymousTypes.ContainsKey(name))
    {
      var allValues = MergeValues(values1, values2);
      var type = AnonymousTypes[name];
      if (type != null)
        newValues = Activator.CreateInstance(type, allValues);
      else
        lock (SyncLock)
        {
          AnonymousTypes.Remove(name);
        }
    }
    else if (name.Length > 1024)
    {
      keyName = CreateHash(name);
      return CreateInstance(keyName, values1, values2, out keyName);
    }

    return newValues;
  }

  private static PropertyDescriptor[] GetProperties(object values1, object values2)
  {
    var properties = new List<PropertyDescriptor>();
    var pdc1 = TypeDescriptor.GetProperties(values1);
    var pdc2 = TypeDescriptor.GetProperties(values2);
    for (var i = 0; i < pdc1.Count; i++)
      if (typeMergerPolicy == null
          || (!typeMergerPolicy.IgnoredProperties.Contains(new Tuple<string, string>(
                values1.GetType().Name,
                pdc1[i].Name))
              && !typeMergerPolicy.UseProperties.Contains(new Tuple<string, string>(
                values2.GetType().Name,
                pdc1[i].Name))))
        properties.Add(pdc1[i]);

    for (var i = 0; i < pdc2.Count; i++)
      if (typeMergerPolicy == null
          || (!typeMergerPolicy.IgnoredProperties.Contains(new Tuple<string, string>(
                values2.GetType().Name,
                pdc2[i].Name))
              && !typeMergerPolicy.UseProperties.Contains(new Tuple<string, string>(
                values1.GetType().Name,
                pdc2[i].Name))))
        properties.Add(pdc2[i]);

    return properties.ToArray();
  }

  private static Type[] GetTypes(PropertyDescriptor[] pdc)
  {
    var types = new List<Type>();
    for (var i = 0; i < pdc.Length; i++) types.Add(pdc[i].PropertyType);

    return types.ToArray();
  }

  private static object[] MergeValues(object values1, object values2)
  {
    var pdc1 = TypeDescriptor.GetProperties(values1);
    var pdc2 = TypeDescriptor.GetProperties(values2);
    var values = new List<object>();
    for (var i = 0; i < pdc1.Count; i++)
      if (typeMergerPolicy == null
          || (!typeMergerPolicy.IgnoredProperties.Contains(new Tuple<string, string>(
                values1.GetType().Name,
                pdc1[i].Name))
              && !typeMergerPolicy.UseProperties.Contains(new Tuple<string, string>(
                values2.GetType().Name,
                pdc1[i].Name))))
        values.Add(pdc1[i].GetValue(values1)!);

    for (var i = 0; i < pdc2.Count; i++)
      if (typeMergerPolicy == null
          || (!typeMergerPolicy.IgnoredProperties.Contains(new Tuple<string, string>(
                values2.GetType().Name,
                pdc2[i].Name))
              && !typeMergerPolicy.UseProperties.Contains(new Tuple<string, string>(
                values1.GetType().Name,
                pdc2[i].Name))))
        values.Add(pdc2[i].GetValue(values2)!);

    return values.ToArray();
  }

  private static void InitializeAssembly()
  {
    if (asmBuilder != null) return;

    var assembly = new AssemblyName
    {
      Name = "AnonymousTypeExtensions"
    };
    asmBuilder = AssemblyBuilder.DefineDynamicAssembly(assembly, AssemblyBuilderAccess.Run);
    modBuilder = asmBuilder.DefineDynamicModule(asmBuilder.GetName().Name!);
  }

  private static Type CreateType(string name, PropertyDescriptor[] pdc)
  {
    var typeBuilder = CreateTypeBuilder(name);
    var types = GetTypes(pdc);
    var fields = BuildFields(typeBuilder, pdc);
    BuildCtor(typeBuilder, fields, types);
    BuildProperties(typeBuilder, fields);
    return typeBuilder.CreateType();
  }

  private static TypeBuilder CreateTypeBuilder(string typeName)
  {
    var typeBuilder = modBuilder!.DefineType(
      typeName,
      TypeAttributes.Public,
      typeof(object));
    return typeBuilder;
  }

  private static string CreateHash(string value)
  {
    lock (SyncLock)
    {
      using (var sha256 = SHA256.Create())
      {
        return Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(value)));
      }
    }
  }

  private static void BuildCtor(TypeBuilder typeBuilder, FieldBuilder[] fields, Type[] types)
  {
    var ctor = typeBuilder.DefineConstructor(
      MethodAttributes.Public,
      CallingConventions.Standard,
      types);
    var ctorGen = ctor.GetILGenerator();
    for (var i = 0; i < fields.Length; i++)
    {
      ctorGen.Emit(OpCodes.Ldarg_0);
      ctorGen.Emit(OpCodes.Ldarg, i + 1);
      ctorGen.Emit(OpCodes.Stfld, fields[i]);
    }

    ctorGen.Emit(OpCodes.Ret);
  }

  private static FieldBuilder[] BuildFields(TypeBuilder typeBuilder, PropertyDescriptor[] pdc)
  {
    var fields = new List<FieldBuilder>();
    for (var i = 0; i < pdc.Length; i++)
    {
      var pd = pdc[i];
      var field = typeBuilder.DefineField(
        string.Format("_{0}", pd.Name),
        pd.PropertyType,
        FieldAttributes.Private);
      if (!fields.Contains(field)) fields.Add(field);
    }

    return fields.ToArray();
  }

  private static void BuildProperties(TypeBuilder typeBuilder, FieldBuilder[] fields)
  {
    for (var i = 0; i < fields.Length; i++)
    {
      var propertyName = fields[i].Name.Substring(1);
      var property = typeBuilder.DefineProperty(
        propertyName,
        PropertyAttributes.None,
        fields[i].FieldType,
        null);
      var getMethod = typeBuilder.DefineMethod(
        string.Format("Get_{0}", propertyName),
        MethodAttributes.Public,
        fields[i].FieldType,
        Type.EmptyTypes);
      var methGen = getMethod.GetILGenerator();
      methGen.Emit(OpCodes.Ldarg_0);
      methGen.Emit(OpCodes.Ldfld, fields[i]);
      methGen.Emit(OpCodes.Ret);
      property.SetGetMethod(getMethod);
    }
  }
}
