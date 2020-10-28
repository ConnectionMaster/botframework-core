// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Core.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Bot.Core.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static void AddBotCore(this IServiceCollection services, IConfiguration configuration)
        {
            if (services == null) { throw new ArgumentNullException(nameof(services)); }
            if (configuration == null) { throw new ArgumentNullException(nameof(configuration)); }

            // Component registrations must be added before the resource explorer is instantiated to ensure
            // that all types are correctly registered. Any types that are registered after the resource explorer
            // is instantiated will not be picked up otherwise.
            //
            ComponentRegistrations.Add();

            ResourceExplorer resourceExplorer = BuildResourceExplorer(
                applicationRoot: configuration.GetSection(ConfigurationConstants.ApplicationRootKey).Value);

            services.AddBotCore(configuration, resourceExplorer);
        }

        internal static void AddBotCore(
            this IServiceCollection services,
            IConfiguration configuration,
            ResourceExplorer resourceExplorer)
        {
            if (services == null) { throw new ArgumentNullException(nameof(services)); }
            if (configuration == null) { throw new ArgumentNullException(nameof(configuration)); }
            if (resourceExplorer == null) { throw new ArgumentNullException(nameof(resourceExplorer)); }

            services.AddSingleton(resourceExplorer);

            Resource runtimeConfigurationResource =
                resourceExplorer.GetResource(id: "runtime.json");
            var runtimeConfigurationProvider =
                resourceExplorer.LoadType<RuntimeConfigurationProvider>(runtimeConfigurationResource);

            runtimeConfigurationProvider.ConfigureServices(services, configuration);
        }

        static ResourceExplorer BuildResourceExplorer(string applicationRoot)
        {
            return new ResourceExplorer()
                .AddFolder(applicationRoot)
                .RegisterType<OnQnAMatch>(OnQnAMatch.Kind);
        }
    }
}
