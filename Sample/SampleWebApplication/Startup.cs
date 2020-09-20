using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReRabbit.Extensions;
using Sample.IntegrationMessages.Messages;
using SampleWebApplication.Extensions;
using SampleWebApplication.Mappers;
using SampleWebApplication.Mappings;
using SampleWebApplication.Middlewares;
using SampleWebApplication.RabbitMq;
using SampleWebApplication.RetryDelayComputers;
using SampleWebApplication.RouteProviders;
using TracingContext.Extensions;

namespace SampleWebApplication
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _env = environment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<TestMiddleware>(); // TODO: scan and register all middlewares ?
            services.AddSingleton<TestMiddleware2>();
            services.AddControllers();
            services
                .AddSwagger(_env)
                .AddAutoMapper(a => a.AddProfiles(AutoMapperConfiguration.GetProfiles()))
                .AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = _configuration.GetConnectionString("RedisConnection");
                    options.InstanceName = _configuration.GetValue<string>("ServiceName");
                })
                .AddSingleton<DefaultMessageMapper>()
                .AddRabbitMq(
                    x =>
                    {
                        x.SubscriberMiddlewares
                            //.AddGlobal<UniqueMessagesSubscriberMiddleware>()
                            .AddFor<TestMessage>()
                                .Add<TestMiddleware3>()
                            .Registrator
                            .AddGlobal<GlobalTestMiddleware>() // adds only for Metrics.
                            .AddFor<Metrics>()
                                .Add<TestMiddleware2>();
                        x.RetryDelayComputerRegistrator.Add<CustomRoundRobinRetryDelayComputer>("CustomRoundRobin");
                        x.RouteProviderRegistrator.Add<MetricsRouteProvider>(nameof(MetricsRabbitMessage));
                        x.Factories.MessageMapper = sp => sp.GetRequiredService<DefaultMessageMapper>();
                    }).AddTraceId((_, settings) =>
                {
                    settings.GenerateIfNotPresent = true;
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseTraceId();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseEndpoints(b => b.MapDefaultControllerRoute());
            app.UseSwagger(_configuration);
        }
    }
}
