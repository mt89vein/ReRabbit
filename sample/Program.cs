using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace SampleWebApplication
{
    public class Program
    {
#pragma warning disable IDE1006 // Naming Styles
        public static async Task Main(string[] args)
#pragma warning restore IDE1006 // Naming Styles
        {
            await CreateWebHostBuilder(args).RunAsync();
        }

        public static IHost CreateWebHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(wb =>
                {
                    wb.UseStartup<Startup>();
                }).Build();
        }
    }
}