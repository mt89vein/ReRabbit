using AutoMapper;
using SampleWebApplication.Mappings.Profiles;
using System.Collections.Generic;

namespace SampleWebApplication.Mappings
{
    /// <summary>
    /// Конфигурация маппинга объектов.
    /// </summary>
    public static class AutoMapperConfiguration
    {
        /// <summary>
        /// Возвращает профили маппинга.
        /// </summary>
        /// <returns>Список профилей маппинга.</returns>
        public static IEnumerable<Profile> GetProfiles()
        {
            return new Profile[]
            {
                new MappingProfile(),
            };
        }
    }
}
