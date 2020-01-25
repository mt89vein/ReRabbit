using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Abstractions.Models;
using ReRabbit.Extensions;
using ReRabbit.Subscribers.Middlewares;
using System.Threading.Tasks;

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

    public class TestMiddleware : MiddlewareBase
    {
        private readonly ILogger<TestMiddleware> _logger;

        public TestMiddleware(ILogger<TestMiddleware> logger)
        {
            _logger = logger;
        }

        public override async Task<Acknowledgement> HandleAsync(MessageContext ctx)
        {
            _logger.LogInformation("before TestMiddleware");
            // before

            var result = await Next(ctx);

            _logger.LogInformation("after TestMiddleware");

            // after

            return result;
        }
    }

    public class TestMiddleware2 : MiddlewareBase
    {
        private readonly ILogger<TestMiddleware2> _logger;

        public TestMiddleware2(ILogger<TestMiddleware2> logger)
        {
            _logger = logger;
        }

        public override async Task<Acknowledgement> HandleAsync(MessageContext ctx)
        {
            _logger.LogInformation("before TestMiddleware2");
            // before

            var result = await Next(ctx);

            _logger.LogInformation("after TestMiddleware2");

            // after

            return result;
        }
    }

    public class GlobalTestMiddleware : MiddlewareBase
    {
        private readonly ILogger<GlobalTestMiddleware> _logger;

        public GlobalTestMiddleware(ILogger<GlobalTestMiddleware> logger)
        {
            _logger = logger;
        }

        public override async Task<Acknowledgement> HandleAsync(MessageContext ctx)
        {
            _logger.LogInformation("before GlobalTestMiddleware");
            // before

            var result = await Next(ctx);

            _logger.LogInformation("after GlobalTestMiddleware");

            // after

            return result;
        }
    }
}
