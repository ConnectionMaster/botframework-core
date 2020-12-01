// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Bot.Builder.Runtime.Extensions;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Microsoft.Bot.Builder.Runtime.Functions.Startup))]

namespace Microsoft.Bot.Builder.Runtime.Functions
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddControllers().AddNewtonsoftJson();

            builder.Services.AddBotRuntime(builder.GetContext().Configuration);
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder configurationBuilder)
        {
            var isDevelopment = configurationBuilder.GetContext().EnvironmentName == "Development";

            configurationBuilder.ConfigurationBuilder.ConfigureBotRuntime(isDevelopment);
        }
    }
}
