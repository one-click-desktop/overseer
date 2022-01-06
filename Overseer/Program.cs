using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using CommandLine.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace OneClickDesktop.Overseer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var parseResult = Parser.Default.ParseArguments<Options>(args);
            parseResult.WithParsed(o => RunOptions(o, args));
            parseResult.WithNotParsed(errs => HandleParseError(parseResult, errs));
        }

        private static void RunOptions(Options opts, string[] args)
        {
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, configuration) =>
                {
                    configuration.Sources.Clear();

                    IHostEnvironment env = hostingContext.HostingEnvironment;

                    configuration
                        .SetBasePath(Path.GetFullPath(opts.ConfigurationFolderPath))
                        .AddIniFile($"appsettings.{env.EnvironmentName}.ini", true, true);

                    Console.WriteLine("Loaded configuration:");
                    foreach ((string key, string value) in
                             configuration.Build().AsEnumerable().Where(t => t.Value is not null))
                    {
                        Console.WriteLine($"{key}={value}");
                    }
                }).ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
                .Build().Run();
        }

        static void HandleParseError<T>(ParserResult<T> result, IEnumerable<Error> errs)
        {
            var builder = SentenceBuilder.Create();
            var errorMessages = HelpText.RenderParsingErrorsTextAsLines(result, builder.FormatError,
                builder.FormatMutuallyExclusiveSetErrors, 1);

            foreach (string s in errorMessages)
                Console.Error.WriteLine(s);
        }
    }
}