using AutoMapper;
using SampleWebApplication.Mappings.Interfaces;
using System;
using System.Linq;
using System.Reflection;

namespace SampleWebApplication.Mappings.Profiles
{
    /// <summary>
    /// Профиль настраивающий маппинги для всех моделей с <see cref="IMappedFrom{T}"/>
    /// </summary>
    public class MappingProfile : Profile
    {
        /// <summary>
        /// Применяет все маппинги в сборке для всех моделей с <see cref="IMappedFrom{T}"/>
        /// </summary>
        public MappingProfile()
        {
            ApplyMappingsFromAssembly(Assembly.GetExecutingAssembly());
        }

        private void ApplyMappingsFromAssembly(Assembly assembly)
        {
            var types = assembly.GetExportedTypes()
                .Where(t => t.GetInterfaces().Any(i =>
                    i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMappedFrom<>)))
                .ToList();

            foreach (var type in types)
            {
                var instance = Activator.CreateInstance(type);
                var methodInfo = type.GetMethod(nameof(IMappedFrom<object>.Mapping));
                methodInfo?.Invoke(instance, new object[] { this });
            }
        }
    }
}