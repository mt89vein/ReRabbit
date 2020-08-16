using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace SampleWebApplication.Extensions
{
    /// <summary>
    /// Методы расширения для <see cref="IApplicationBuilder"/>.
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Настройка документирования (Swagger)
        /// </summary>
        /// <param name="app">Конфигурация конвейера запросов.</param>
        /// <param name="configuration">Конфигурация.</param>
        public static void UseSwagger(this IApplicationBuilder app, IConfiguration configuration)
        {
            app.UseSwagger()
                .UseSwaggerUI(c =>
                {
                    c.DocumentTitle = configuration["ServiceName"];
                    c.RoutePrefix = "api-docs";
                    c.SwaggerEndpoint("/swagger/rerabbit-api/swagger.json", "rerabbit-api");
                });
        }

    }
}