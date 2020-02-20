using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReRabbit.Extensions;
using SampleWebApplication.Middlewares;
using SampleWebApplication.RetryDelayComputers;

namespace SampleWebApplication
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();
            services.AddSingleton<TestMiddleware>();
            services.AddSingleton<TestMiddleware2>();
            services.AddSingleton<GlobalTestMiddleware>();
            services.AddDistributedRedisCache(options =>
            {
                options.Configuration = _configuration.GetConnectionString("RedisConnection");
                options.InstanceName = _configuration.GetValue<string>("ServiceName");
            });

            services.AddRabbitMq(
                x =>
                {
                    x.SubscriberPlugins
                        //.Add<UniqueMessagesSubscriberPlugin>(global:true)
                        .Add<GlobalTestMiddleware>(global: true)
                        .Add<TestMiddleware>()
                        .Add<TestMiddleware2>();

                    x.RetryDelayComputerRegistrator.Add<CustomRoundRobinRetryDelayComputer>("CustomRoundRobin");
                });

            services.AddControllers();
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
            app.UseRabbitMq();
        }
    }
}
