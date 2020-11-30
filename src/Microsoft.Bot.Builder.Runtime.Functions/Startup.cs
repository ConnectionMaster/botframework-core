// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IO;
using System.Reflection;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Bot.Builder.Runtime;
using Microsoft.Bot.Builder.Runtime.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Microsoft.Bot.Builder.Runtime.Functions.Startup))]

namespace Microsoft.Bot.Builder.Runtime.Functions
{
    public class Startup : FunctionsStartup
    {
        private const string AppSettingsRelativePath = @"appsettings.json";

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddControllers().AddNewtonsoftJson();

            builder.Services.AddBotCore(builder.GetContext().Configuration);
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder configurationBuilder)
        {
            var hostBuilderContext = configurationBuilder.GetContext();
            var isDevelopment = hostBuilderContext.EnvironmentName == "Development";

            // Use Composer bot path adapter
            configurationBuilder.ConfigurationBuilder.AddBotCoreConfiguration(
                applicationRoot: Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                isDevelopment: isDevelopment);

            IConfiguration configuration = configurationBuilder.ConfigurationBuilder.Build();

            string botRootPath = configuration.GetValue<string>(ConfigurationConstants.BotKey);
            string configFilePath = Path.GetFullPath(Path.Combine(botRootPath, AppSettingsRelativePath));

            configurationBuilder.ConfigurationBuilder.AddJsonFile(configFilePath, optional: true, reloadOnChange: true);

            // Use Composer luis and qna settings extensions
            configurationBuilder.ConfigurationBuilder.AddComposerConfiguration();

            configurationBuilder.ConfigurationBuilder.AddEnvironmentVariables();
        }
    }
}
