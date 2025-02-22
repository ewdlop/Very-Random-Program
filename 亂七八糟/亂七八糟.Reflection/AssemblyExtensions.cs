using System.Reflection;
using System.Runtime.InteropServices;

namespace 亂七八糟.Reflection
{
    public static class AssemblyExtensions
    {
        public static IEnumerable<Type> AsTypes(this Assembly assembly, string attribute)
        {
            return assembly.GetTypes()
                .Where(t => t.GetCustomAttributes().Any(a => a.GetType().Name == attribute));
        }

        public static IEnumerable<Type> AsTypes(this Assembly assembly, Attribute attribute)
        {
            return assembly.GetTypes()
                .Where(t => t.GetCustomAttributes().Any(a => a.GetType().Name == attribute.GetType().Name));
        }

        public static IEnumerable<Type> AsClasses(this Assembly assembly, string attribute)
        {
            return assembly.GetTypes()
                .Where(t => t.IsClass && t.GetCustomAttributes().Any(a => a.GetType().Name == attribute.GetType().Name));
        }

        public static IEnumerable<Type> AsClasses(this Assembly assembly, Attribute attribute)
        {
            return assembly.GetTypes()
                .Where(t => t.IsClass && t.GetCustomAttributes().Any(a => a.GetType().Name == attribute.GetType().Name));
        }

        public static IEnumerable<MethodInfo> AsMethodInfoEnumerable<T>(this Assembly assembly, string attribute)
        {
            return assembly.GetTypes().Where(t => t.GetCustomAttributes().Any(a => a.GetType().Name == attribute))
                .SelectMany(t => t.GetMethods())
                .Where(m => m.GetCustomAttributes().Any(a => a.GetType().Name == attribute));
        }


        public static ConstructorInfo? AsConstructorInfo(this Assembly assembly,
            BindingFlags bindingFlags = BindingFlags.Default,
            Binder? binder = null,
            CallingConventions callingConventions = CallingConventions.Any,
            Type[]? parameterTypes = null,
            ParameterModifier[]? parameterModifiers = null)
        {
            return assembly.GetType().GetConstructor(bindingFlags, binder, callingConventions, parameterTypes ?? [], parameterModifiers ?? []);
        }

        public static IEnumerable<ConstructorInfo> AsConstructorInfoEnumeralbe(this Assembly assembly,
            string attribute,
            BindingFlags bindingFlags = BindingFlags.Default)
        {
            return assembly.GetType().GetConstructors(bindingFlags)
                .Where(c => c.GetCustomAttributes().Any(a => a.GetType().Name == attribute));
        }
    }
}