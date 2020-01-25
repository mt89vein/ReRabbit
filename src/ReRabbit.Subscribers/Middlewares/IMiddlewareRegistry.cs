namespace ReRabbit.Subscribers.Middlewares
{
    /// <summary>
    /// Реестр middleware.
    /// </summary>
    public interface IMiddlewareRegistry
    {
        /// <summary>
        /// Зарегистрировать middleware.
        /// </summary>
        /// <typeparam name="TMiddleware">Тип middleware.</typeparam>
        /// <returns>
        /// Реестр плагинов.
        /// </returns>
        IMiddlewareRegistry Add<TMiddleware>(bool global = false)
            where TMiddleware : class, IMiddleware;


        // TODO: add Type перегрузку
    }
}