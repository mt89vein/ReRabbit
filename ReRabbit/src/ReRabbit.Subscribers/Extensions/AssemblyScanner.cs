using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ReRabbit.Subscribers.Extensions
{
    /// <summary>
    /// Вспомогательные методы для сканирования сборки.
    /// </summary>
    public static class AssemblyScanner
    {
        /// <summary>
        /// Получает все типы, которые реализуют интерфейс (generic, or not generic).
        /// Если тип реализует 2 интерфейса, он будет продублирован.
        /// </summary>
        /// <param name="implementedInterface">Тип интерфейса. Например IMessageHandler{T}.</param>
        /// <returns>Список типов, реализующий указанный интерфейс.</returns>
        public static IEnumerable<Type> GetClassesImplementingAnInterface(Type implementedInterface)
        {
            if (!implementedInterface.IsInterface)
            {
                throw new ArgumentException("Передан некорректный тип");
            }

            IEnumerable<Type> typesInTheAssembly;

            try
            {
                typesInTheAssembly = AppDomain.CurrentDomain
                                              .GetAssemblies()
                                              .SelectMany(s => s.GetTypes());
            }
            catch (ReflectionTypeLoadException e)
            {
                typesInTheAssembly = e.Types.Where(t => t != null);
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
                            yield return typeInTheAssembly;
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
                        yield return typeInTheAssembly;
                    }
                }
            }
        }

        /// <summary>
        /// Зарегистрировать все типы, которые реализуют указанный интерфейс.
        /// </summary>
        /// <param name="services">Регистратор сервисов.</param>
        /// <param name="interfaceType">Интерфейс.</param>
        /// <param name="lifetime">Время жизни объекта.</param>
        public static IServiceCollection AddClassesAsImplementedInterface(
            this IServiceCollection services,
            Type interfaceType,
            ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            if (!interfaceType.IsInterface && !interfaceType.IsGenericType)
            {
                throw new ArgumentException("Передан некорректный тип для регистрации");
            }

            var typeInfos = AppDomain.CurrentDomain
                                     .GetAssemblies()
                                     .SelectMany(s => s.GetTypes())
                                     .Where(p => p.IsClass && !p.IsAbstract &&
                                                 p.GetInterfaces()
                                                     .Any(i => i.IsGenericType &&
                                                               i.GetGenericTypeDefinition() == interfaceType))
                                     .Select(t => t.GetTypeInfo());

            foreach (var type in typeInfos)
            {
                // регистрируем конкретный тип.
                switch (lifetime)
                {
                    case ServiceLifetime.Scoped:
                        services.AddScoped(type);
                        break;
                    case ServiceLifetime.Singleton:
                        services.AddSingleton(type);
                        break;
                    case ServiceLifetime.Transient:
                        services.AddTransient(type);
                        break;
                }

                // регистрируем конкретный тип через его интерфейсы,
                // чтобы в рантайме резолвился один и тот же инстанс для любого из его интерфейсов.
                // этот момент быть очень важен для Scoped и Singleton.
                foreach (var implementedInterface in type.ImplementedInterfaces)
                {
                    switch (lifetime)
                    {
                        case ServiceLifetime.Scoped:
                            services.AddScoped(implementedInterface, sp => sp.GetService(type));
                            break;
                        case ServiceLifetime.Singleton:
                            services.AddSingleton(implementedInterface, sp => sp.GetService(type));
                            break;
                        case ServiceLifetime.Transient:
                            services.AddTransient(implementedInterface, type);
                            break;
                    }
                }
            }

            return services;
        }
    }
}
