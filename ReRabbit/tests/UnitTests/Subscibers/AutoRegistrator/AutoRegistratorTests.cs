using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using ReRabbit.Extensions;
using ReRabbit.Extensions.Registrator;
using ReRabbit.Subscribers.Consumers;
using ReRabbit.Subscribers.Exceptions;
using ReRabbit.Subscribers.Middlewares;
using ReRabbit.UnitTests.TestFiles;
using System;
using System.Linq;

namespace ReRabbit.UnitTests.Subscibers.AutoRegistrator
{
    [TestOf(typeof(RabbitMqHandlerAutoRegistrator))]
    public class AutoRegistratorTests
    {
        [Test]
        public void ShouldAddOneConsumer()
        {
            var services = new ServiceCollection();
            services.AddRabbitMq();
            services.AddSingleton<NormalConsumer.TestRabbitMessage>();
            services.AddConfiguration();
            services.AddFakes();
            var sp = services.BuildServiceProvider();

            var registrator = new RabbitMqHandlerAutoRegistrator(
                sp,
                typeFilter: x => x.Namespace == "ReRabbit.UnitTests.Subscibers.AutoRegistrator.NormalConsumer"
            );
            var consumerRegistry = sp.GetRequiredService<IConsumerRegistry>();
            registrator.ScanAndRegister();

            Assert.AreEqual(1, (consumerRegistry as IConsumerRegistryAccessor)?.Consumers?.Count);
        }

        [Test]
        public void ShouldThrowNotSupportedExceptionOnMultipleConsumersOnSingleQueue()
        {
            var services = new ServiceCollection();
            services.AddRabbitMq();
            services.AddSingleton<MultipleConsumersOnSingleQueue.SecondTestRabbitMessage>();
            services.AddConfiguration();
            services.AddFakes();

            var registrator = new RabbitMqHandlerAutoRegistrator(
                services.BuildServiceProvider(),
                typeFilter: x => x.Namespace == "ReRabbit.UnitTests.Subscibers.AutoRegistrator.MultipleConsumersOnSingleQueue"
            );

            Assert.Throws<NotSupportedException>(() => registrator.ScanAndRegister());
        }

        [Test]
        public void ShouldThrowExceptionIfWithoutConfigurationAttribute()
        {
            var services = new ServiceCollection();
            services.AddRabbitMq();
            services.AddSingleton<NotConfiguredConsumer.TestRabbitMessage>();
            services.AddConfiguration();
            services.AddFakes();

            var registrator = new RabbitMqHandlerAutoRegistrator(
                services.BuildServiceProvider(),
                typeFilter: x =>
                    x.Namespace == "ReRabbit.UnitTests.Subscibers.AutoRegistrator.NotConfiguredConsumer"
            );

            Assert.Throws<SubscriberNotConfiguredException>(() => registrator.ScanAndRegister());
        }

        [Test]
        public void ShouldRespectMultipleConfigurationAttributesOnSingleConsumer()
        {
            var services = new ServiceCollection();
            services.AddRabbitMq();
            services.AddSingleton<MultipleConfigurationAttributes.TestRabbitMessage>();
            services.AddConfiguration();
            services.AddFakes();
            var sp = services.BuildServiceProvider();

            var registrator = new RabbitMqHandlerAutoRegistrator(
                sp,
                typeFilter: x => x.Namespace == "ReRabbit.UnitTests.Subscibers.AutoRegistrator.MultipleConfigurationAttributes"
            );
            var consumerRegistry = sp.GetRequiredService<IConsumerRegistry>();
            registrator.ScanAndRegister();

            Assert.AreEqual(3, (consumerRegistry as IConsumerRegistryAccessor)?.Consumers?.Count);
        }

        [Test]
        public void ShouldAddConsumersWithMiddlewares()
        {
            #region Arrange

            var services = new ServiceCollection();
            services.AddRabbitMq(
                options => options.SubscriberMiddlewares
                    .AddFor<ConsumersWithMiddlewares.TestHandler, ConsumersWithMiddlewares.TestMessageDto>()
                        .Add<ConsumersWithMiddlewares.Middleware2>()
                    .Registrator
                    .AddGlobal<ConsumersWithMiddlewares.GlobalMiddleware>() // добавится только для TestMessage2Dto
                    .AddFor<ConsumersWithMiddlewares.TestHandler2, ConsumersWithMiddlewares.TestMessage2Dto>()
                        .Add<ConsumersWithMiddlewares.Middleware1>()
            );
            services.AddSingleton<ConsumersWithMiddlewares.TestRabbitMessage>();
            services.AddSingleton<ConsumersWithMiddlewares.TestRabbitMessage2>();
            services.AddConfiguration();
            services.AddFakes();
            var sp = services.BuildServiceProvider();

            var registrator = new RabbitMqHandlerAutoRegistrator(
                sp,
                typeFilter: x => x.Namespace == "ReRabbit.UnitTests.Subscibers.AutoRegistrator.ConsumersWithMiddlewares"
            );
            var consumerRegistry = sp.GetRequiredService<IConsumerRegistry>();
            var middlewareRegistryAccessor = sp.GetRequiredService<IMiddlewareRegistryAccessor>();

            #endregion Arrange

            #region Act

            registrator.ScanAndRegister();

            #endregion Act

            #region Assert

            var testMessageMiddlewares = middlewareRegistryAccessor.Get(
                typeof(ConsumersWithMiddlewares.TestHandler), typeof(ConsumersWithMiddlewares.TestMessageDto));
            var testMessage2Middlewares = middlewareRegistryAccessor.Get(
                typeof(ConsumersWithMiddlewares.TestHandler2), typeof(ConsumersWithMiddlewares.TestMessage2Dto)).ToList();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(2, (consumerRegistry as IConsumerRegistryAccessor)?.Consumers.Count);
                Assert.AreEqual(1, testMessageMiddlewares.Count, "Количество Middleware не совпадает для TestMessageDto.");
                Assert.IsTrue(testMessageMiddlewares.All(x => x.MiddlewareType != typeof(ConsumersWithMiddlewares.GlobalMiddleware)), "Для TestMessageDto нет глобальных Middleware.");
                Assert.IsTrue(testMessageMiddlewares.All(x => x.MiddlewareType != typeof(ConsumersWithMiddlewares.GlobalMiddleware)), "Для TestMessageDto нет глобальных Middleware.");

                Assert.AreEqual(3, testMessage2Middlewares.Count, "Количество Middleware не совпадает для TestMessage2Dto.");
                Assert.IsTrue(testMessage2Middlewares.Any(x => x.MiddlewareType == typeof(ConsumersWithMiddlewares.GlobalMiddleware)), "Для TestMessage2Dto есть глобальный Middleware.");
            });

            #endregion Assert
        }

        [Test]
        public void ShouldAddConsumersWithMiddlewaresAndRespectExecutionOrder()
        {
            #region Arrange

            var services = new ServiceCollection();
            services.AddRabbitMq(
                options => options.SubscriberMiddlewares
                    .AddFor<ConsumersWithMiddlewares.TestHandler, ConsumersWithMiddlewares.TestMessageDto>()
                        .Add<ConsumersWithMiddlewares.Middleware2>()
                    .Registrator
                    .AddGlobal<ConsumersWithMiddlewares.GlobalMiddleware>() // добавится только для TestMessage2Dto
                    .AddFor<ConsumersWithMiddlewares.TestHandler2, ConsumersWithMiddlewares.TestMessage2Dto>()
                        .Add<ConsumersWithMiddlewares.Middleware1>(executionOrder: 2)
                        .Add<ConsumersWithMiddlewares.Middleware2>(executionOrder: 3) // мидлварь зареган тут, но и добавлен в хендлер. То что указано здесь является преимуществом.
                    .Registrator
                    .AddFor<ConsumersWithMiddlewares.TestHandler2, ConsumersWithMiddlewares.TestMessage2Dto>() // повторное выполнение не приведет к обнулению реестра мидлварей.
                        .Add<ConsumersWithMiddlewares.Middleware1>() // добавится только тот, что был выше с executionOrder: 2
            );
            services.AddSingleton<ConsumersWithMiddlewares.TestRabbitMessage>();
            services.AddSingleton<ConsumersWithMiddlewares.TestRabbitMessage2>();
            services.AddConfiguration();
            services.AddFakes();
            var sp = services.BuildServiceProvider();

            var registrator = new RabbitMqHandlerAutoRegistrator(
                sp,
                typeFilter: x => x.Namespace == "ReRabbit.UnitTests.Subscibers.AutoRegistrator.ConsumersWithMiddlewares"
            );
            var consumerRegistry = sp.GetRequiredService<IConsumerRegistry>();
            var middlewareRegistryAccessor = sp.GetRequiredService<IMiddlewareRegistryAccessor>();

            #endregion Arrange

            #region Act

            registrator.ScanAndRegister();

            #endregion Act

            #region Assert

            var testMessageMiddlewares = middlewareRegistryAccessor.Get(
                typeof(ConsumersWithMiddlewares.TestHandler),typeof(ConsumersWithMiddlewares.TestMessageDto));
            var testMessage2Middlewares = middlewareRegistryAccessor.Get(
                typeof(ConsumersWithMiddlewares.TestHandler2),typeof(ConsumersWithMiddlewares.TestMessage2Dto)).ToList();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(2, (consumerRegistry as IConsumerRegistryAccessor)?.Consumers.Count);
                Assert.AreEqual(1, testMessageMiddlewares.Count, "Количество Middleware не совпадает для TestMessageDto.");
                Assert.IsTrue(testMessageMiddlewares.All(x => x.MiddlewareType != typeof(ConsumersWithMiddlewares.GlobalMiddleware)), "Для TestMessageDto нет глобальных Middleware.");
                Assert.IsTrue(testMessageMiddlewares.All(x => x.MiddlewareType != typeof(ConsumersWithMiddlewares.GlobalMiddleware)), "Для TestMessageDto нет глобальных Middleware.");

                Assert.AreEqual(3, testMessage2Middlewares.Count, "Количество Middleware не совпадает для TestMessage2Dto.");
                Assert.IsTrue(testMessage2Middlewares.Any(x => x.MiddlewareType == typeof(ConsumersWithMiddlewares.GlobalMiddleware)), "Для TestMessage2Dto есть глобальный Middleware.");

                Assert.AreEqual(typeof(ConsumersWithMiddlewares.GlobalMiddleware), testMessage2Middlewares[0].MiddlewareType, "Первым выполнится глобальный");
                Assert.AreEqual(typeof(ConsumersWithMiddlewares.Middleware1), testMessage2Middlewares[1].MiddlewareType, "Вторым Middleware1");
                Assert.AreEqual(typeof(ConsumersWithMiddlewares.Middleware2), testMessage2Middlewares[2].MiddlewareType, "Третьим Middleware2");
            });

            #endregion Assert
        }
    }
}