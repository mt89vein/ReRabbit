using AutoMapper;

namespace SampleWebApplication.Mappings.Interfaces
{
    /// <summary>
    /// Интерфейс для обозначения необходимости маппинга из указанного типа-источника (Т)
    /// </summary>
    /// <typeparam name="T">Тип-источник</typeparam>
    public interface IMappedFrom<T> where T : new()
    {
        /// <summary>
        /// Настройка маппинга.
        /// </summary>
        /// <param name="profile">Профиль маппинга.</param>
        void Mapping(Profile profile);
    }
}
