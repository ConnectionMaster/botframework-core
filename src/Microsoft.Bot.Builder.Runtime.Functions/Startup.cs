﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Bot.Builder.Runtime.Extensions;

[assembly: FunctionsStartup(typeof(Microsoft.Bot.Builder.Runtime.FunctionHost.Startup))]

namespace Microsoft.Bot.Builder.Runtime.FunctionHost
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddBotRuntime(builder.GetContext().Configuration);
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder configurationBuilder)
        {
            var isDevelopment = string.Equals(
                configurationBuilder.GetContext().EnvironmentName,
                Microsoft.Extensions.Hosting.Environments.Development,
                StringComparison.OrdinalIgnoreCase);
            var applicationRoot = configurationBuilder.GetContext().ApplicationRootPath;

            configurationBuilder.ConfigurationBuilder.AddBotRuntimeConfiguration(applicationRoot, isDevelopment);
        }
    }
}
