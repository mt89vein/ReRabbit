using ReRabbit.Abstractions.Models;

namespace ReRabbit.Abstractions
{
    /// <summary>
    /// Предоставляет информацию о сервисе.
    /// </summary>
    public interface IServiceInfoAccessor
    {
        /// <summary>
        /// Информация о сервисе.
        /// </summary>
        ServiceInfo ServiceInfo { get; }
    }
}
