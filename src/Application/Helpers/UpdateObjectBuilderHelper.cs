using System.Reflection;
using System.Windows.Markup;

namespace Application.Helpers;

public static class UpdateObjectBuilderHelper
{
  public static IDictionary<string, object> BuildUpdateObject<T>(T source)
  {
    var result = new Dictionary<string, object>();
    var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

    foreach (var prop in props)
    {
      var value = prop.GetValue(source);
      if (HasValue(value, prop.PropertyType)) result[prop.Name] = value!;
    }

    return result;
  }

  private static bool HasValue(object? value, Type type)
  {
    if (value is null) return false;
    if (type.IsValueType)
    {
      var defaultValue = Activator.CreateInstance(type);
      return !value.Equals(defaultValue);
    }

    return true;
  }
}