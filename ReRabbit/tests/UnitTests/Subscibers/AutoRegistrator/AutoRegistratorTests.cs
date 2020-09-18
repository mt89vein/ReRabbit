using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using ReRabbit.Extensions;
using ReRabbit.Subscribers;
using ReRabbit.Subscribers.Exceptions;
using System;

namespace ReRabbit.UnitTests.Subscibers.AutoRegistrator
{
    [TestOf(typeof(RabbitMqHandlerAutoRegistrator))]
    public class AutoRegistratorTests
    {
        [Test]
        public void ShouldAddOneConsumer()
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile("TestFiles/appsettings.json", optional: false);
            var configuration = configurationBuilder.Build();

            var services = new ServiceCollection();
            services.AddRabbitMq();
            services.AddSingleton<NormalConsumer.TestRabbitMessage>();
            services.AddSingleton<IConfiguration>(configuration);
            var sp = services.BuildServiceProvider();

            var registrator = new RabbitMqHandlerAutoRegistrator(sp);
            var consumerRegistry = sp.GetRequiredService<IConsumerRegistry>();
            registrator.FillConsumersRegistry(consumerRegistry, x => x.Namespace == "ReRabbit.UnitTests.Subscibers.AutoRegistrator.NormalConsumer");

            Assert.AreEqual(1, (consumerRegistry as IConsumerRegistryAccessor)?.Consumers?.Count);
        }

        [Test]
        public void ShouldThrowNotSupportedExceptionOnMultipleConsumersOnSingleQueue()
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile("TestFiles/appsettings.json", optional: false);
            var configuration = configurationBuilder.Build();

            var services = new ServiceCollection();
            services.AddRabbitMq();
            services.AddSingleton<MultipleConsumersOnSingleQueue.SecondTestRabbitMessage>();
            services.AddSingleton<IConfiguration>(configuration);
            var sp = services.BuildServiceProvider();

            var registrator = new RabbitMqHandlerAutoRegistrator(sp);
            var consumerRegistry = sp.GetRequiredService<IConsumerRegistry>();

            Assert.Throws<NotSupportedException>(() => registrator.FillConsumersRegistry(consumerRegistry,
                x => x.Namespace == "ReRabbit.UnitTests.Subscibers.AutoRegistrator.MultipleConsumersOnSingleQueue")
            );
        }

        [Test]
        public void ShouldThrowExceptionIfWithoutConfigurationAttribute()
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile("TestFiles/appsettings.json", optional: false);
            var configuration = configurationBuilder.Build();

            var services = new ServiceCollection();
            services.AddRabbitMq();
            services.AddSingleton<NotConfiguredConsumer.TestRabbitMessage>();
            services.AddSingleton<IConfiguration>(configuration);
            var sp = services.BuildServiceProvider();

            var registrator = new RabbitMqHandlerAutoRegistrator(sp);
            var consumerRegistry = sp.GetRequiredService<IConsumerRegistry>();

            Assert.Throws<SubscriberNotConfiguredException>(() => registrator.FillConsumersRegistry(consumerRegistry,
                x => x.Namespace == "ReRabbit.UnitTests.Subscibers.AutoRegistrator.NotConfiguredConsumer")
            );
        }

        [Test]
        public void ShouldRespectMultipleConfigurationAttributesOnSingleConsumer()
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile("TestFiles/appsettings.json", optional: false);
            var configuration = configurationBuilder.Build();

            var services = new ServiceCollection();
            services.AddRabbitMq();
            services.AddSingleton<MultipleConfigurationAttributes.TestRabbitMessage>();
            services.AddSingleton<IConfiguration>(configuration);
            var sp = services.BuildServiceProvider();

            var registrator = new RabbitMqHandlerAutoRegistrator(sp);
            var consumerRegistry = sp.GetRequiredService<IConsumerRegistry>();
            registrator.FillConsumersRegistry(consumerRegistry, x => x.Namespace == "ReRabbit.UnitTests.Subscibers.AutoRegistrator.MultipleConfigurationAttributes");

            Assert.AreEqual(3, (consumerRegistry as IConsumerRegistryAccessor)?.Consumers?.Count);
        }
    }
}