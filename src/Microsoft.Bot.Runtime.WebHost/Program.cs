// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Bot.Runtime.WebHost
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, builder) =>
            {
                IHostEnvironment env = hostingContext.HostingEnvironment;

                // Use Composer bot path adapter
                builder.UseBotPathConverter(
                    applicationRoot: Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    isDevelopment: env.IsDevelopment());

                IConfiguration configuration = builder.Build();

                string configFilePath = Path.GetFullPath(
                    path: Path.Combine(
                        configuration.GetValue<string>(ConfigurationConstants.BotKey),
                        @"settings/appsettings.json"));

                builder.AddJsonFile(configFilePath, optional: true, reloadOnChange: true);

                // Use Composer luis and qna settings extensions
                builder.UseComposerSettings();

                builder.AddEnvironmentVariables()
                    .AddCommandLine(args);
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
    }
}
