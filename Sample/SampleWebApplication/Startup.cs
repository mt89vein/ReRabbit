using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReRabbit.Extensions;
using SampleWebApplication.Extensions;
using SampleWebApplication.Mappers;
using SampleWebApplication.Mappings;
using SampleWebApplication.Middlewares;
using SampleWebApplication.RabbitMq;
using SampleWebApplication.RetryDelayComputers;

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
                            .Add<TestMiddleware>()
                            .Add<TestMiddleware2>()
                        .Registry
                        .AddGlobal<GlobalTestMiddleware>() // adds only for Metrics.
                        .AddFor<Metrics>()
                            .Add<TestMiddleware2>();
                    x.RetryDelayComputerRegistrator.Add<CustomRoundRobinRetryDelayComputer>("CustomRoundRobin");
                    x.Factories.MessageMapper = sp => sp.GetRequiredService<DefaultMessageMapper>();
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
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
