using ReRabbit.Abstractions.Settings.Connection;
using System.Diagnostics.CodeAnalysis;

namespace ReRabbit.Core.Settings.Connection
{
    /// <summary>
    /// Настройки виртуального хоста.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal sealed class VirtualHostSettingsDto
    {
        /// <summary>
        /// Наименование виртуального хоста.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Имя пользователя.
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// Пароль.
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// Использовать общую очередь с ошибочными сообщениями.
        /// </summary>
        public bool? UseCommonErrorMessagesQueue { get; set; }

        /// <summary>
        /// Использовать общую очередь с ошибочным роутингом (те что не ушли ни в одну из других очередей из-за отсутствия биндинга).
        /// </summary>
        public bool? UseCommonUnroutedMessagesQueue { get; set; }

        public VirtualHostSettings Create(ConnectionSettings connectionSettings)
        {
            return new VirtualHostSettings(
                connectionSettings,
                Name,
                UserName,
                Password,
                UseCommonErrorMessagesQueue,
                UseCommonUnroutedMessagesQueue
            );
        }
    }
}