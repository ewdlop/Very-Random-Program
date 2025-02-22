using System.Reflection;

namespace 亂七八糟.Reflection
{
    public static class TypeExtensions
    {
        public static bool? HasAttribute(this Type? type, string attribute)
        {
            return type?.Attributes.HasAttribute(attribute);
        }
        public static bool? HasAttribute<T>(this Type? type, Attribute attribute)
        {
            return type?.Attributes.HasAttribute(attribute.GetType().Name);
        }

        public static bool? HasAttribute<T>(this T? type, string attribute)
        {
            return type?.GetType().Attributes.HasAttribute(attribute);
        }

        public static object? ToObject<T>(this Type? type, 
            string attribute,
            BindingFlags bindingFlags = BindingFlags.Default,
            object[]? objects = null)
        {
            return type?.GetConstructors( bindingFlags = BindingFlags.Default)
                .FirstOrDefault(c => c.HasAttribute(attribute) ?? false)?.Invoke(objects);
        }
    }
}