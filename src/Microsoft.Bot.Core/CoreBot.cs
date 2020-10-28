// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Core.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.Bot.Core
{
    public class CoreBot : ActivityHandler
    {
        private const string DefaultLocale = "en-US";

        private readonly ConversationState conversationState;
        private readonly DialogManager dialogManager;
        private readonly bool removeRecipientMention;
        private readonly UserState userState;

        public CoreBot(IServiceProvider services, IOptions<CoreBotOptions> options)
        {
            this.conversationState = services.GetRequiredService<ConversationState>();
            this.userState = services.GetRequiredService<UserState>();

            /*
             * TODO: Define and implement replacement of RemoveRecipientMention feature
             * BODY: RemoveRecipientMention appears to be a Teams-related Activity extension that removes @mentions in ,
             * this should be decoupled from the core runtime and available as a middleware.
             */
            this.removeRecipientMention = options.Value.RemoveRecipientMention;

            this.dialogManager = CreateDialogManager(services, options);
        }

        static DialogManager CreateDialogManager(IServiceProvider services, IOptions<CoreBotOptions> options)
        {
            var resourceExplorer = services.GetRequiredService<ResourceExplorer>();
            var telemetryClient = services.GetService<IBotTelemetryClient>();

            Resource rootDialogResource = resourceExplorer.GetResource(options.Value.RootDialog);
            var rootDialog = resourceExplorer.LoadType<AdaptiveDialog>(rootDialogResource);

            var dialogManager = new DialogManager(rootDialog)
                .UseResourceExplorer(resourceExplorer)
                .UseLanguageGeneration()
                .UseLanguagePolicy(new LanguagePolicy(options.Value.DefaultLocale ?? DefaultLocale));

            if (telemetryClient != null)
            {
                dialogManager.UseTelemetry(telemetryClient);
            }

            dialogManager.InitialTurnState.Set(services.GetRequiredService<BotFrameworkClient>());
            dialogManager.InitialTurnState.Set(services.GetRequiredService<SkillConversationIdFactoryBase>());

            return dialogManager;
        }

        public override async Task OnTurnAsync(
            ITurnContext turnContext,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            AdaptiveDialog rootDialog = (AdaptiveDialog)this.dialogManager.RootDialog;
            if (turnContext.TurnState.Get<IIdentity>(BotAdapter.BotIdentityKey) is ClaimsIdentity claimIdentity &&
                SkillValidation.IsSkillClaim(claimIdentity.Claims))
            {
                rootDialog.AutoEndDialog = true;
            }

            if (this.removeRecipientMention && turnContext?.Activity?.Type == ActivityTypes.Message)
            {
                turnContext.Activity.RemoveRecipientMention();
            }

            await this.dialogManager.OnTurnAsync(turnContext, cancellationToken: cancellationToken);
            await this.conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await this.userState.SaveChangesAsync(turnContext, false, cancellationToken);
        }
    }
}
