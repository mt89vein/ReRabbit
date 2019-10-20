using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReRabbit.Abstractions;
using ReRabbit.Abstractions.Acknowledgements;
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
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var q1Subscriber = loggerFactory.CreateLogger("Q1Subscriber");

            var subscriptionManager = app.ApplicationServices.GetService<ISubscriptionManager>();

            subscriptionManager.Register<TestMessage>((x, y) =>
            {
                q1Subscriber.LogInformation(x.Message);

                return Task.FromResult<Acknowledgement>(new Reject(null, null, false));
            }, "Q1Subscriber");

            //subscriptionManager.Register<TestMessage>((x, y) =>
            //{
            //    Console.WriteLine("1" + x.Message);
            //    return Task.FromResult<Acknowledgement>(new Ack());
            //}, "Q2Subscriber", "DefaultConnection", "TESTHOST");

            //subscriptionManager.Register<TestMessage>((x, y) =>
            //{
            //    Console.WriteLine("1" + x.Message);
            //    return Task.FromResult<Acknowledgement>(new Ack());
            //}, "Q2Subscriber", "DefaultConnection", "TESTHOST");

            //subscriptionManager.Register<TestMessage>((x, y) =>
            //{
            //    Console.WriteLine("1" + x.Message);
            //    return Task.FromResult<Acknowledgement>(new Ack());
            //}, "Q3Subscriber", "SecondConnection", "ThirdVirtualHost");

            //subscriptionManager.Register<TestMessage>((x, y) =>
            //{
            //    Console.WriteLine("1" + x.Message);
            //    return Task.FromResult<Acknowledgement>(new Nack());
            //}, "Q3Subscriber", "SecondConnection", "ThirdVirtualHost");
        }
    }

    public class TestMessage
    {
        public string Message { get; set; }
    }
}
