using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReRabbit.Abstractions;
using ReRabbit.Extensions;
using System;
using System.Threading.Tasks;

namespace SampleWebApplication
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();
            services.AddRabbitMq();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var subscriptionManager = app.ApplicationServices.GetService<ISubscriptionManager>();

            subscriptionManager.Register<TestMessage>((x, y) =>
            {
                Console.WriteLine("1" + x.Message);
                return Task.CompletedTask;
            }, "Q1Subscriber");

            subscriptionManager.Register<TestMessage>((x, y) =>
            {
                Console.WriteLine("1" + x.Message);
                return Task.CompletedTask;
            }, "Q1Subscriber");

            subscriptionManager.Register<TestMessage>((x, y) =>
            {
                Console.WriteLine("1" + x.Message);
                return Task.CompletedTask;
            }, "Q2Subscriber", "DefaultConnection", "TESTHOST");

            subscriptionManager.Register<TestMessage>((x, y) =>
            {
                Console.WriteLine("1" + x.Message);
                return Task.CompletedTask;
            }, "Q3Subscriber", "SecondConnection", "ThirdVirtualHost");

            subscriptionManager.Register<TestMessage>((x, y) =>
            {
                Console.WriteLine("1" + x.Message);
                return Task.CompletedTask;
            }, "Q3Subscriber", "SecondConnection", "ThirdVirtualHost");
        }
    }

    public class TestMessage
    {
        public string Message { get; set; }
    }
}
