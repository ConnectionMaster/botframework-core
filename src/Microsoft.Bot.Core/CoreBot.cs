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
            IConfiguration configuration,
            ConversationState conversationState,
            UserState userState,
            ResourceExplorer resourceExplorer,
            BotFrameworkClient skillClient,
            SkillConversationIdFactoryBase conversationIdFactory,
            IBotTelemetryClient telemetryClient)
        {
            this.conversationState = conversationState;
            this.userState = userState;
            this.dialogState = conversationState.CreateProperty<DialogState>("DialogState");
            this.resourceExplorer = resourceExplorer;
            this.defaultLocale = configuration.GetValue<string>("defaultLanguage") ?? "en-us"; ;
            this.telemetryClient = telemetryClient;

            // TODO: Could this not be a setting (as opposed to a hard coded location)?
            this.rootDialogFile = GetRootDialog(configuration["bot"]);

            // TODO: Probably a better way to do this
            var features = new BotFeatureSettings();
            configuration.GetSection("feature").Bind(features);
            this.removeRecipientMention = features.RemoveRecipientMention;

            this.LoadRootDialog();
            this.dialogManager.InitialTurnState.Set(skillClient);
            this.dialogManager.InitialTurnState.Set(conversationIdFactory);
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
