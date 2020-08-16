using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.IO;

namespace SampleWebApplication.Extensions
{
    /// <summary>
    /// Методы расширения для <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Настройка документирования (Swagger).
        /// </summary>
        /// <param name="services">Провайдер служб.</param>
        /// <param name="env">Переменные окружения.</param>
        public static IServiceCollection AddSwagger(this IServiceCollection services, IHostEnvironment env)
        {
            services.AddSwaggerGenNewtonsoftSupport();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("rerabbit-api", new OpenApiInfo
                {
                    Description = "Тестовые методы для публикации сообщений",
                    Title = "ReRabbitApi",
                    Version = "rerabbit-api"
                });

                options.IgnoreObsoleteProperties();

                Array.ForEach(new[] { "SampleWebApplication.xml" }, xml =>
                {
                    var xmlPath = Path.Combine(env.ContentRootPath, xml);
                    options.IncludeXmlComments(xmlPath);
                });
            });

            return services;
        }
    }
}
