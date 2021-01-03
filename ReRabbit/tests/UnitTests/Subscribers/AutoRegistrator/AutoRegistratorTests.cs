using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using ReRabbit.Extensions;
using ReRabbit.Extensions.Registrator;
using ReRabbit.Subscribers.Consumers;
using ReRabbit.Subscribers.Exceptions;
using ReRabbit.Subscribers.Middlewares;
using ReRabbit.UnitTests.Subscribers.AutoRegistrator.ConsumersWithMiddlewares;
using ReRabbit.UnitTests.TestFiles;
using System;
using System.Linq;

namespace ReRabbit.UnitTests.Subscribers.AutoRegistrator
{
    [TestOf(typeof(RabbitMqHandlerAutoRegistrator))]
    public class AutoRegistratorTests
    {
        [TestCase("NormalConsumer", 1)]
        [TestCase("MultipleConsumersOnDifferentQueues", 2)]
        [TestCase("MultipleConfigurationAttributes", 3)]
        [TestCase("MultipleConsumersOnSingleQueue", 0, typeof(NotSupportedException))]
        [TestCase("NotConfiguredConsumer", 0, typeof(SubscriberNotConfiguredException))]
        public void ShouldCorrectlyScanAndRegister(string @namespace, int consumerCount, Type? exceptionType = null)
        {
            #region Arrange

            var services = new ServiceCollection();
            services.AddRabbitMq();
            services.AddConfiguration();
            services.AddFakes();
            var sp = services.BuildServiceProvider();

            var registrator = new RabbitMqHandlerAutoRegistrator(
                sp,
                typeFilter: x => x.Namespace == "ReRabbit.UnitTests.Subscribers.AutoRegistrator." + @namespace
            );

            #endregion Arrange

            #region Assert

            void AssertionCode() => registrator.ScanAndRegister();

            #endregion Assert

            #region Assert

            Assert.Multiple(() =>
            {
                if (exceptionType is not null)
                {
                    Assert.Throws(
                        exceptionType,
                        AssertionCode,
                        "Регистрация должна выбросить исключение."
                    );
                }
                else
                {
                    Assert.DoesNotThrow(AssertionCode, "Регистрация не должна выбросывать исключение.");
                    Assert.AreEqual(
                        consumerCount,
                        sp.GetRequiredService<IConsumerRegistryAccessor>().Consumers.Count,
                        "Количество зарегистрированных потребителей не совпадает."
                    );
                }
            });

            #endregion Assert
        }

        [Test]
        public void ShouldAddConsumersWithMiddlewares()
        {
            #region Arrange

            var services = new ServiceCollection();
            services.AddRabbitMq(
                options => options.SubscriberMiddlewares
                    .AddFor<TestHandler, TestMessageDto>()
                        .Add<Middleware2>()
                    .Registrator
                    .AddGlobal<GlobalMiddleware>() // добавится только для TestMessage2Dto
                    .AddFor<TestHandler2, TestMessage2Dto>()
                        .Add<Middleware1>()
            );
            services.AddConfiguration();
            services.AddFakes();
            var sp = services.BuildServiceProvider();

            var registrator = new RabbitMqHandlerAutoRegistrator(
                sp,
                typeFilter: x => x.Namespace == "ReRabbit.UnitTests.Subscribers.AutoRegistrator.ConsumersWithMiddlewares"
            );
            var consumerRegistry = sp.GetRequiredService<IConsumerRegistry>();
            var middlewareRegistryAccessor = sp.GetRequiredService<IMiddlewareRegistryAccessor>();

            #endregion Arrange

            #region Act

            registrator.ScanAndRegister();

            #endregion Act

            #region Assert

            var testMessageMiddlewares =
                middlewareRegistryAccessor.Get(typeof(TestHandler), typeof(TestMessageDto));
            var testMessage2Middlewares =
                middlewareRegistryAccessor.Get(typeof(TestHandler2), typeof(TestMessage2Dto)).ToList();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(2, (consumerRegistry as IConsumerRegistryAccessor)?.Consumers.Count);
                Assert.AreEqual(1, testMessageMiddlewares.Count, "Количество Middleware не совпадает для TestMessageDto.");
                Assert.IsTrue(testMessageMiddlewares.All(x => x.MiddlewareType != typeof(GlobalMiddleware)), "Для TestMessageDto нет глобальных Middleware.");
                Assert.IsTrue(testMessageMiddlewares.All(x => x.MiddlewareType != typeof(GlobalMiddleware)), "Для TestMessageDto нет глобальных Middleware.");

                Assert.AreEqual(3, testMessage2Middlewares.Count, "Количество Middleware не совпадает для TestMessage2Dto.");
                Assert.IsTrue(testMessage2Middlewares.Any(x => x.MiddlewareType == typeof(GlobalMiddleware)), "Для TestMessage2Dto есть глобальный Middleware.");
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
                    .AddFor<TestHandler, TestMessageDto>()
                        .Add<Middleware2>()
                    .Registrator
                    .AddGlobal<GlobalMiddleware>() // добавится только для TestMessage2Dto
                    .AddFor<TestHandler2, TestMessage2Dto>()
                        .Add<Middleware1>(executionOrder: 2)
                        .Add<Middleware2>(executionOrder: 3) // мидлварь зареган тут, но и добавлен в хендлер. То что указано здесь является преимуществом.
                    .Registrator
                    .AddFor<TestHandler2, ConsumersWithMiddlewares.TestMessage2Dto>() // повторное выполнение не приведет к обнулению реестра мидлварей.
                        .Add<Middleware1>() // добавится только тот, что был выше с executionOrder: 2
            );
            services.AddConfiguration();
            services.AddFakes();
            var sp = services.BuildServiceProvider();

            var registrator = new RabbitMqHandlerAutoRegistrator(
                sp,
                typeFilter: x => x.Namespace == "ReRabbit.UnitTests.Subscribers.AutoRegistrator.ConsumersWithMiddlewares"
            );
            var consumerRegistry = sp.GetRequiredService<IConsumerRegistry>();
            var middlewareRegistryAccessor = sp.GetRequiredService<IMiddlewareRegistryAccessor>();

            #endregion Arrange

            #region Act

            registrator.ScanAndRegister();

            #endregion Act

            #region Assert

            var testMessageMiddlewares =
                middlewareRegistryAccessor.Get(typeof(TestHandler),typeof(TestMessageDto));
            var testMessage2Middlewares =
                middlewareRegistryAccessor.Get(typeof(TestHandler2),typeof(TestMessage2Dto)).ToList();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(2, (consumerRegistry as IConsumerRegistryAccessor)?.Consumers.Count);
                Assert.AreEqual(1, testMessageMiddlewares.Count, "Количество Middleware не совпадает для TestMessageDto.");
                Assert.IsTrue(testMessageMiddlewares.All(x => x.MiddlewareType != typeof(GlobalMiddleware)), "Для TestMessageDto нет глобальных Middleware.");
                Assert.IsTrue(testMessageMiddlewares.All(x => x.MiddlewareType != typeof(GlobalMiddleware)), "Для TestMessageDto нет глобальных Middleware.");

                Assert.AreEqual(3, testMessage2Middlewares.Count, "Количество Middleware не совпадает для TestMessage2Dto.");
                Assert.IsTrue(testMessage2Middlewares.Any(x => x.MiddlewareType == typeof(GlobalMiddleware)), "Для TestMessage2Dto есть глобальный Middleware.");

                Assert.AreEqual(typeof(GlobalMiddleware), testMessage2Middlewares[0].MiddlewareType, "Первым выполнится глобальный");
                Assert.AreEqual(typeof(Middleware1), testMessage2Middlewares[1].MiddlewareType, "Вторым Middleware1");
                Assert.AreEqual(typeof(Middleware2), testMessage2Middlewares[2].MiddlewareType, "Третьим Middleware2");
            });

            #endregion Assert
        }
    }
}