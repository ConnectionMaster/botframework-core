// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq.Expressions;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Core.Builders.Middleware;
using Microsoft.Bot.Core.Extensions;
using Microsoft.Bot.Core.Settings;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Bot.Core
{
    public class CoreBotAdapter : BotFrameworkHttpAdapter
    {
        public CoreBotAdapter(
            IServiceProvider services,
            IConfiguration configuration,
            IOptions<CoreBotAdapterOptions> options,
            ILogger<BotFrameworkHttpAdapter> logger)
            : base(
                services.GetService<ICredentialProvider>(),
                services.GetService<AuthenticationConfiguration>(),
                services.GetService<IChannelProvider>(),
                logger: services.GetService<ILogger<BotFrameworkHttpAdapter>>())
        {
            var conversationState = services.GetService<ConversationState>();
            var userState = services.GetService<UserState>();

            var rexplorer = services.GetService<ResourceExplorer>();

            this.UseStorage(services.GetService<IStorage>());
            this.UseBotState(userState, conversationState);
            this.Use(new RegisterClassMiddleware<IConfiguration>(configuration));

            foreach (IMiddlewareBuilder middleware in options.Value.Middleware)
            {
                this.Use(middleware.Build(services, configuration));
            }

            this.OnTurnError = options.Value.OnTurnError.Build(services, configuration, logger);
        }
    }
}
