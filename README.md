# ReRabbit

![UnitTest](https://github.com/mt89vein/ReRabbit/workflows/UnitTest/badge.svg?event=push&maxAge=600)
[![Coverage Status](https://coveralls.io/repos/github/mt89vein/ReRabbit/badge.svg?branch=master&maxAge=600)](https://coveralls.io/github/mt89vein/ReRabbit?branch=master)

RabbitMQ framework over official RabbitMQ.Client

### Features highlight

-   JSON конфигурация, clean startup
-   near-zero C# code configuration
-   точки расширения на всех этапах обработки
    -   Serializer, with JSON.NET by default
    -   Acknowlegdement behaviour
    -   Route provider for publisher
    -   Middlewares
    -   Subscriber
    -   Retry delay computers
    -   Naming conventions
    -   Topology provider
    -   Connection manager
    -   Configuration provider
    -   etc
-   dead-letter message support
-   catch common error messages via policy
-   catch unrouted messages via policy
-   delayed publish support
-   retry and retry with delay support via broker
-   poisoned message handling
-   rich scaling settings
-   enabled distributed tracing
-   autoscan and register handlers by convention
-   IServiceScope created for every message, then resolves IMessageHandler from it 
-   autorecovering all handlers after disconnect
-   publisher confirms
-   separate connections for publishers and subscribers
-   thread-safe singleton publisher with channel pooling

### Install

Для запуска просто клонируем проект в папку и запускаем в docker через CLI:

```bash
docker-compose -f docker-compose.yml -f docker-compose.override.yml up
```

либо в Visual Studio / Rider запускаем docker-compose.

Еще вариант, поднять RabbitMQ и Redis (для дедупликатора) в docker

```bash
docker-compose -f docker-compose.debug.yml up
```

либо установить и запустить на локальной машине и потом в IIS/Kestrel запустить само приложение [Sample](https://github.com/mt89vein/ReRabbit/tree/master/Sample)

Далее проект с примером будет доступен по адресу http://localhost:5000

### Nuget package

... coming soon

### Example

in startup.cs

```diff
public void ConfigureServices(IServiceCollection services)
{
+   services.AddRabbitMq(); // можно настроить сервисы через лямбду.
}
```

in appsettings.json 

```jsonc
{
    "RabbitMq": {
        "SubscriberConnections": {
            "DefaultConnection": {
                "VirtualHosts": {
                    "/": {
                        "Queues": {
                            "MetricsSubscriber": {
                                "QueueName": "app-metrics",
                                "Bindings": [
                                    {
                                        "FromExchange": "metrics",
                                        "ExchangeType": "fanout"
                                    }
                                ],
                                // dead letter, durability, autoack etc
                                // retry settings
                                // scaling settings
                                // tracing settings
                            }
                        }
                        // other queue settings
                    }
                    // other virtual host settings 
                }
                // other connection settings
            }
        },
        // publisher connections and message settings
    }
}
```
минимальный конфиг - название очереди и биндинги.  
Детали в примерах, полный конфиг в [JSON schema](https://github.com/mt89vein/ReRabbit/tree/master/Sample/SampleWebApplication/JsonSchemas)

in MetricsSubscriber.cs

```cs

public class MetricsDto : IntegrationMessage // или IMessage
{
    public string Label { get; set; }
    public int Value { get; set; }
}


/// <summary>
/// Пример обработчика метрик.
/// <summary>
public class MetricsSubscriber : IMessageHandler<MetricsDto> // реализуем интерфейс
{
    // и указываем название подписчика из конфигурации
    // При этом можно указать несколько атрибутов они все автоматически начнут
    // обработку, в том числе из разных очередей, виртуальных хостов или даже инстансов RabbitMQ.
    [SubscriberConfiguration("MetricsSubscriber")]
    public async Task<Acknowledgement> HandleAsync(MessageContext<MetricsDto> ctx)
    {
        await DoSomethingAsync(ctx.Message);

        // явно указываем, что сообщение обработано
        return Ack.Ok;

        // возможные варианты из коробки: Reject, Retry.In etc
        // можно расширить, добавив своего наследника и заменив AcknowledgementBehaviour
    }
}
```
