// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Declarative;
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

            AddComponentRegistrations(services, configuration);

            ResourceExplorer resourceExplorer = BuildResourceExplorer(
                applicationRoot: configuration.GetSection(ConfigurationConstants.ApplicationRootKey).Value);
            services.AddSingleton(resourceExplorer);

            Resource runtimeConfigurationResource =
                resourceExplorer.GetResource(id: "runtime.json");
            var runtimeConfigurationProvider =
                resourceExplorer.LoadType<RuntimeConfigurationProvider>(runtimeConfigurationResource);

            runtimeConfigurationProvider.ConfigureServices(services, configuration);
        }

        static void AddComponentRegistrations(IServiceCollection services, IConfiguration configuration)
        {
            ComponentRegistration.Add(new DialogsComponentRegistration());
            ComponentRegistration.Add(new DeclarativeComponentRegistration());
            ComponentRegistration.Add(new AdaptiveComponentRegistration());
            ComponentRegistration.Add(new LanguageGenerationComponentRegistration());
            ComponentRegistration.Add(new QnAMakerComponentRegistration());
            ComponentRegistration.Add(new LuisComponentRegistration());
            ComponentRegistration.Add(new CoreBotComponentRegistration());
        }

        static ResourceExplorer BuildResourceExplorer(string applicationRoot)
        {
            return new ResourceExplorer()
                .AddFolder(applicationRoot)
                .RegisterType<OnQnAMatch>(OnQnAMatch.Kind);
        }
    }
}
