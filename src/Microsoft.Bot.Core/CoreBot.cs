// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.Bot.Core
{
    public class CoreBot : ActivityHandler
    {
        private readonly ConversationState conversationState;
        private readonly string defaultLocale;
        private DialogManager dialogManager;
        private readonly IStatePropertyAccessor<DialogState> dialogState;
        private readonly bool removeRecipientMention;
        private readonly ResourceExplorer resourceExplorer;
        private readonly string rootDialogFile;
        private readonly IBotTelemetryClient telemetryClient;
        private readonly UserState userState;

        public CoreBot(
            IServiceProvider services,
            IConfiguration configuration,
            IOptions<CoreBotOptions> options)
        {
            this.conversationState = services.GetService<ConversationState>();
            this.userState = services.GetService<UserState>();
            this.dialogState = conversationState.CreateProperty<DialogState>("DialogState");
            this.resourceExplorer = services.GetService<ResourceExplorer>();
            this.defaultLocale = options.Value.DefaultLocale ?? "en-US";
            this.telemetryClient = services.GetService<IBotTelemetryClient>();

            /*
             * TODO: Runtime should get the root dialog path through application settings rather than hard-coded location
             * BODY: Define and implement a method for getting the root dialog path through application settings.
             */
            this.rootDialogFile = GetRootDialog(options.Value.RootDialog);

            /*
             * TODO: Runtime shouldn't bind bot feature settings to hard-coded class
             * BODY: Define and implement a replacement for today's implementation of BotFeatureSettings.
             */

            /*
             * TODO: Define and implement replacement of RemoveRecipientMention feature
             * BODY: RemoveRecipientMention appears to be a Teams-related Activity extension that removes @mentions in , this should be decoupled from the core runtime and available as a middleware. 
             */
            this.removeRecipientMention = options.Value.RemoveRecipientMention;

            this.LoadRootDialog();
            this.dialogManager.InitialTurnState.Set(services.GetService<BotFrameworkClient>());
            this.dialogManager.InitialTurnState.Set(services.GetService<SkillConversationIdFactoryBase>());
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            AdaptiveDialog rootDialog = (AdaptiveDialog)this.dialogManager.RootDialog;
            if (turnContext.TurnState.Get<IIdentity>(BotAdapter.BotIdentityKey) is ClaimsIdentity claimIdentity && SkillValidation.IsSkillClaim(claimIdentity.Claims))
            {
                rootDialog.AutoEndDialog = true;
            }

            if (this.removeRecipientMention && turnContext?.Activity?.Type == "message")
            {
                turnContext.Activity.RemoveRecipientMention();
            }

            await this.dialogManager.OnTurnAsync(turnContext, cancellationToken: cancellationToken);
            await this.conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await this.userState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        private string GetRootDialog(string folderPath)
        {
            var dir = new DirectoryInfo(folderPath);
            foreach (var f in dir.GetFiles())
            {
                if (f.Extension == ".dialog")
                {
                    return f.Name;
                }
            }

            throw new Exception($"Can't locate root dialog in {dir.FullName}");
        }

        private void LoadRootDialog()
        {
            var rootFile = this.resourceExplorer.GetResource(rootDialogFile);
            var rootDialog = this.resourceExplorer.LoadType<AdaptiveDialog>(rootFile);
            this.dialogManager = new DialogManager(rootDialog)
                                .UseResourceExplorer(resourceExplorer)
                                .UseLanguageGeneration()
                                .UseLanguagePolicy(new LanguagePolicy(defaultLocale));

            if (this.telemetryClient != null)
            {
                this.dialogManager.UseTelemetry(this.telemetryClient);
            }
        }
    }
}
