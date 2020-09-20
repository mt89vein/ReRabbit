using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ReRabbit.UnitTests.TestFiles
{
    public static class ConfigurationHelper
    {
        public static IConfiguration GetConfiguration(string fileName = "appsettings.json")
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile("TestFiles/" + fileName, optional: false);
            return configurationBuilder.Build();
        }

        public static IServiceCollection AddConfiguration(this IServiceCollection services, string fileName = "appsettings.json")
        {
            return services.AddSingleton(GetConfiguration(fileName));
        }
    }
}
