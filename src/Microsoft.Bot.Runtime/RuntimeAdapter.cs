// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Integration.ApplicationInsights.Core;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Runtime.Settings;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Runtime
{
    public class RuntimeAdapter : BotFrameworkHttpAdapter
    {
        public RuntimeAdapter(
                IConfiguration configuration,
                IStorage storage,
                UserState userState,
                ConversationState conversationState,
                TelemetryInitializerMiddleware telemetryInitializerMiddleware)
            : base(configuration)
        {
            this.UseStorage(storage);
            this.UseBotState(userState, conversationState);
            Use(new RegisterClassMiddleware<IConfiguration>(configuration));

            Use(telemetryInitializerMiddleware);

            var features = new BotFeatureSettings();
            configuration.GetSection("feature").Bind(features);

            var blobStorage = new BlobStorageConfiguration();
            configuration.GetSection("blobStorage").Bind(blobStorage);

            if (BotSettings.ConfigSectionValid(blobStorage?.ConnectionString) &&
                BotSettings.ConfigSectionValid(blobStorage?.Container))
            {
                Use(new TranscriptLoggerMiddleware(
                    new AzureBlobTranscriptStore(
                        blobStorage?.ConnectionString,
                        blobStorage?.Container)));
            }

            if (features.UseInspectionMiddleware)
            {
                Use(new InspectionMiddleware(new InspectionState(storage)));
            }

            if (features.UseShowTypingMiddleware)
            {
                Use(new ShowTypingMiddleware());
            }

            OnTurnError = async (turnContext, exception) =>
            {
                await turnContext.SendActivityAsync(exception.Message).ConfigureAwait(false);
                await conversationState.ClearStateAsync(turnContext).ConfigureAwait(false);
                await conversationState.SaveChangesAsync(turnContext).ConfigureAwait(false);
            };
        }
    }
}