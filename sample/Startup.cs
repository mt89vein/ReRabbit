using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReRabbit.Abstractions.Acknowledgements;
using ReRabbit.Extensions;
using ReRabbit.Subscribers.Models;
using ReRabbit.Subscribers.Plugins;
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
            services.AddScoped<TestPlugin>();
            services.AddScoped<TestPlugin2>();
            services.AddScoped<GlobalTestPlugin>();
            services.AddDistributedRedisCache(options =>
            {
                options.Configuration = _configuration.GetConnectionString("RedisConnection");
                options.InstanceName = _configuration.GetValue<string>("ServiceName");
            });

            services.AddRabbitMq(
                x => x.SubscriberPlugins
                    //.Add<UniqueMessagesSubscriberPlugin>(global:true)
                    .Add<GlobalTestPlugin>(global: true)
                    .Add<TestPlugin>()
                    .Add<TestPlugin2>()
            );

            services.AddMvcCore(options => options.EnableEndpointRouting = false);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRabbitMq();
            app.UseMvc().UseMvcWithDefaultRoute();
        }
    }

    public class TestPlugin : MiddlewareBase
    {
        private readonly ILogger<TestPlugin> _logger;

        public TestPlugin(ILogger<TestPlugin> logger)
        {
            _logger = logger;
        }

        public override async Task<Acknowledgement> HandleAsync(MessageContext ctx)
        {
            _logger.LogInformation("before TestPlugin");
            // before

            var result = await Next(ctx);

            _logger.LogInformation("after TestPlugin");

            // after

            return result;
        }
    }

    public class TestPlugin2 : MiddlewareBase
    {
        private readonly ILogger<TestPlugin2> _logger;

        public TestPlugin2(ILogger<TestPlugin2> logger)
        {
            _logger = logger;
        }

        public override async Task<Acknowledgement> HandleAsync(MessageContext ctx)
        {
            _logger.LogInformation("before TestPlugin2");
            // before

            var result = await Next(ctx);

            _logger.LogInformation("after TestPlugin2");

            // after

            return result;
        }
    }

    public class GlobalTestPlugin : MiddlewareBase
    {
        private readonly ILogger<GlobalTestPlugin> _logger;

        public GlobalTestPlugin(ILogger<GlobalTestPlugin> logger)
        {
            _logger = logger;
        }

        public override async Task<Acknowledgement> HandleAsync(MessageContext ctx)
        {
            _logger.LogInformation("before GlobalTestPlugin");
            // before

            var result = await Next(ctx);

            _logger.LogInformation("after GlobalTestPlugin");

            // after

            return result;
        }
    }
}
