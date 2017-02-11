using System;
using System.Collections.Generic;
using LiveObjects.Utils.ExtensionMethods;

namespace LiveObjects.DependencyInjection
{
    public static class Dependency
    {
        public static Dictionary<Type, Func<object>> Factories { get; } = new Dictionary<Type, Func<object>>();

        public static void Register<T>(T obj)
        {
            Register(() => obj);
        }

        public static void Register<T>(Func<T> generator)
        {
            Factories[typeof(T)] = () => generator();
        }

        public static T Get<T>(bool throwIfNotFound = true)
        {
            var factory = Factories.GetValueOrDefault(typeof(T));
            if(factory == null && throwIfNotFound)
                throw new Exception($"Dependecy not found: {typeof(T).Name}");
            return factory == null ? default(T) : (T) factory();
        }
    }
}