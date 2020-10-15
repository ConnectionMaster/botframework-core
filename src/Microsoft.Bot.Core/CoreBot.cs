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
using Microsoft.Bot.Core.Settings;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Core
{
    public class CoreBot : ActivityHandler
    {
        private readonly ConversationState _conversationState;
        private readonly string _defaultLocale;
        private DialogManager _dialogManager;
        private readonly IStatePropertyAccessor<DialogState> _dialogState;
        private readonly bool _removeRecipientMention;
        private readonly ResourceExplorer _resourceExplorer;
        private readonly string _rootDialogFile;
        private readonly IBotTelemetryClient _telemetryClient;
        private readonly UserState _userState;

        public CoreBot(
            IConfiguration configuration,
            ConversationState conversationState,
            UserState userState,
            ResourceExplorer resourceExplorer,
            BotFrameworkClient skillClient,
            SkillConversationIdFactoryBase conversationIdFactory,
            IBotTelemetryClient telemetryClient)
        {
            this._conversationState = conversationState;
            this._userState = userState;
            this._dialogState = conversationState.CreateProperty<DialogState>("DialogState");
            this._resourceExplorer = resourceExplorer;
            this._defaultLocale = configuration.GetValue<string>("defaultLanguage") ?? "en-us";
            this._telemetryClient = telemetryClient;

            /*
             * TODO: Runtime should get the root dialog path through application settings rather than hard-coded location
             * BODY: Define and implement a method for getting the root dialog path through application settings.
             */
            this._rootDialogFile = GetRootDialog(configuration["bot"]);

            /*
             * TODO: Runtime shouldn't bind bot feature settings to hard-coded class
             * BODY: Define and implement a replacement for today's implementation of BotFeatureSettings.
             */
            var features = new BotFeatureSettings();
            configuration.GetSection("feature").Bind(features);

            /*
             * TODO: Define and implement replacement of RemoveRecipientMention feature
             * BODY: RemoveRecipientMention appears to be a Teams-related Activity extension that removes @mentions in , this should be decoupled from the core runtime and available as a middleware. 
             */
            this._removeRecipientMention = features.RemoveRecipientMention;

            this.LoadRootDialog();
            this._dialogManager.InitialTurnState.Set(skillClient);
            this._dialogManager.InitialTurnState.Set(conversationIdFactory);
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            AdaptiveDialog rootDialog = (AdaptiveDialog)this._dialogManager.RootDialog;
            if (turnContext.TurnState.Get<IIdentity>(BotAdapter.BotIdentityKey) is ClaimsIdentity claimIdentity && SkillValidation.IsSkillClaim(claimIdentity.Claims))
            {
                rootDialog.AutoEndDialog = true;
            }

            if (this._removeRecipientMention && turnContext?.Activity?.Type == "message")
            {
                turnContext.Activity.RemoveRecipientMention();
            }

            await this._dialogManager.OnTurnAsync(turnContext, cancellationToken: cancellationToken).ConfigureAwait(false);
            await this._conversationState.SaveChangesAsync(turnContext, false, cancellationToken).ConfigureAwait(false);
            await this._userState.SaveChangesAsync(turnContext, false, cancellationToken).ConfigureAwait(false);
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
            var rootFile = this._resourceExplorer.GetResource(_rootDialogFile);
            var rootDialog = this._resourceExplorer.LoadType<AdaptiveDialog>(rootFile);
            this._dialogManager = new DialogManager(rootDialog)
                                .UseResourceExplorer(_resourceExplorer)
                                .UseLanguageGeneration()
                                .UseLanguagePolicy(new LanguagePolicy(_defaultLocale));

            if (this._telemetryClient != null)
            {
                this._dialogManager.UseTelemetry(this._telemetryClient);
            }
        }
    }
}
