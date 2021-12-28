using System;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace OneClickDesktop.Overseer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, configuration) =>
                {
                    configuration.Sources.Clear();

                    IHostEnvironment env = hostingContext.HostingEnvironment;

                    configuration
                        .AddIniFile("appsettings.ini", optional: true, reloadOnChange: true)
                        .AddIniFile($"appsettings.{env.EnvironmentName}.ini", true, true);

                    if (env.EnvironmentName == "Development")
                    {
                        foreach ((string key, string value) in
                            configuration.Build().AsEnumerable().Where(t => t.Value is not null))
                        {
                            Console.WriteLine($"{key}={value}");
                        }
                    }
                }).ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
