// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core.Skills;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Core.Providers.Adapter;
using Microsoft.Bot.Core.Providers.Storage;
using Microsoft.Bot.Core.Providers.Telemetry;
using Microsoft.Bot.Core.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using IChannelProvider = Microsoft.Bot.Core.Providers.Channel.IChannelProvider;
using ICredentialProvider = Microsoft.Bot.Core.Providers.Credentials.ICredentialProvider;

namespace Microsoft.Bot.Core.Providers
{
    [JsonObject]
    public class RuntimeConfigurationProvider : IProvider
    {
        [JsonProperty("adapters")]
        public IList<IAdapterProvider> Adapters { get; } = new List<IAdapterProvider>();

        [JsonProperty("channel")]
        public IChannelProvider Channel { get; set; }

        [JsonProperty("credentials")]
        public ICredentialProvider Credentials { get; set; }

        [JsonProperty("defaultLocale")]
        public string DefaultLocale { get; set; }

        [JsonProperty("removeRecipientMention")]
        public bool RemoveRecipientMention { get; set; }

        [JsonProperty("rootDialog")]
        public string RootDialog { get; set; }

        [JsonProperty("storage")]
        public IStorageProvider Storage { get; set; }

        [JsonProperty("telemetry")]
        public ITelemetryProvider Telemetry { get; set; }

        static void ConfigureAuthenticationConfigurationServices(
            IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddSingleton<AuthenticationConfiguration>();
        }

        static void ConfigureBotStateServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<UserState>();
            services.AddSingleton<ConversationState>();
        }

        void ConfigureCoreBotServices(IServiceCollection services, IConfiguration configuration)
        {
            var options = new CoreBotOptions
            {
                DefaultLocale = this.DefaultLocale,
                RemoveRecipientMention = this.RemoveRecipientMention,
                RootDialog = this.RootDialog
            };

            services.ConfigureOptions(options);

            services.AddSingleton<IBot, CoreBot>();
        }

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            if (services == null) { throw new ArgumentNullException(nameof(services)); }
            if (configuration == null) { throw new ArgumentNullException(nameof(configuration)); }

            var providers = new List<IProvider>(this.Adapters);
            providers.AddRange(new IProvider[]
            {
                this.Channel,
                this.Credentials,
                this.Storage,
                this.Telemetry
            });

            foreach (IProvider provider in providers)
            {
                provider.ConfigureServices(services, configuration);
            }

            ConfigureSkillServices(services, configuration);
            ConfigureBotStateServices(services, configuration);
            ConfigureAuthenticationConfigurationServices(services, configuration);
            ConfigureCoreBotServices(services, configuration);
        }

        static void ConfigureSkillServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<SkillConversationIdFactoryBase, SkillConversationIdFactory>();
            services.AddHttpClient<BotFrameworkClient, SkillHttpClient>();
            services.AddSingleton<ChannelServiceHandler, SkillHandler>();
        }
    }
}
