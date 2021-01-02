using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ReRabbit.Extensions.Helpers
{
    /// <summary>
    /// Вспомогательные методы для сканирования сборки.
    /// </summary>
    internal static class AssemblyScanner
    {
        /// <summary>
        /// Получает все типы, которые реализуют интерфейс (generic, or not generic).
        /// Если тип реализует 2 интерфейса, он будет продублирован.
        /// </summary>
        /// <param name="implementedInterface">Тип интерфейса. Например IMessageHandler{T}.</param>
        /// <param name="filter">Фильтр типов.</param>
        /// <param name="assemblies">Сборки для сканирования.</param>
        /// <returns>Список типов, реализующий указанный интерфейс.</returns>
        public static IEnumerable<Type> GetClassesImplementingAnInterface(
            Type implementedInterface,
            Assembly[] assemblies,
            Func<Type, bool>? filter = null
        )
        {
            if (!implementedInterface.IsInterface)
            {
                throw new ArgumentException("Передан некорректный тип");
            }

            IEnumerable<Type> typesInTheAssembly;

            try
            {
                typesInTheAssembly = assemblies.SelectMany(s => s.GetTypes());
            }
            catch (ReflectionTypeLoadException e)
            {
                typesInTheAssembly = e.Types.Where(t => t != null).Cast<Type>();
            }

            typesInTheAssembly = typesInTheAssembly.Where(t => t.IsClass && !t.IsAbstract);

            if (implementedInterface.IsGenericType)
            {
                var genericTypeDefinition = implementedInterface.GetGenericTypeDefinition();

                foreach (var typeInTheAssembly in typesInTheAssembly)
                {
                    var typeInterfaces = typeInTheAssembly.GetInterfaces();
                    foreach (var typeInterface in typeInterfaces)
                    {
                        if (typeInterface.IsGenericType &&
                            typeInterface.GetGenericTypeDefinition() == genericTypeDefinition)
                        {
                            if (filter?.Invoke(typeInTheAssembly) != false)
                            {
                                yield return typeInTheAssembly;
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (var typeInTheAssembly in typesInTheAssembly)
                {
                    if (implementedInterface.IsAssignableFrom(typeInTheAssembly))
                    {
                        if (filter?.Invoke(typeInTheAssembly) != false)
                        {
                            yield return typeInTheAssembly;
                        }
                    }
                }
            }
        }
    }
}
