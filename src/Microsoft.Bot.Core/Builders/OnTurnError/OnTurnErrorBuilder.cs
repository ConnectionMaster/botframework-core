using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Core.Extensions;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.Bot.Core.Builders.OnTurnError
{
    /// <summary>
    /// When added, this configures an OnTurnError implementation for your bot adapter.
    /// You can toggle whether to log the exception and send a trace activity with the stack trace, as well
    /// as provide LG templates for the logged exception message and the bot response to the user.
    /// </summary>
    public class OnTurnErrorBuilder : IOnTurnErrorBuilder
    {
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.OnTurnErrorProvider";

        /// <summary>
        /// Gets or sets the boolean flag to log errors. Defaults to false.
        /// </summary>
        /// <value>
        /// The LogError property gets/sets the value of the bool expression, logError.
        /// </value>
        [JsonProperty("logError")]
        public BoolExpression LogError { get; set; }

        /// <summary>
        /// Gets or sets the boolean flag to send a trace activity with the stack trace to the user. Defaults to false.
        /// </summary>
        /// <value>
        /// The LogError property gets/sets the value of the bool expression, sendTraceActivity.
        /// </value>
        [JsonProperty("sendTraceActivity")]
        public BoolExpression SendTraceActivity { get; set; }

        public Func<ITurnContext, Exception, Task> Build(IServiceProvider services, IConfiguration configuration)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var conversationState = services.GetService<ConversationState>();

            return async (turnContext, exception) =>
            {
                // Log any leaked exception from the application.
                if (this.LogError?.GetConfigurationValue(configuration) ?? false)
                {
                    var logger = services.GetService<ILogger<BotFrameworkHttpAdapter>>();

                    logger.LogError(exception, exception.Message);
                }

                // Send a trace activity with the exception to the user.
                if (this.SendTraceActivity?.GetConfigurationValue(configuration) ?? false)
                {
                    await turnContext.SendActivityAsync(new Activity(type: ActivityTypes.Trace, text: exception.StackTrace)).ConfigureAwait(false);
                }

                // Send the exception message to the user. Since the default behavior does not
                // send logs or trace activities, the bot appears hanging without any activity
                // to the user.
                await turnContext.SendActivityAsync(exception.Message).ConfigureAwait(false);

                // Delete the conversationState for the current conversation to prevent the
                // bot from getting stuck in a error-loop caused by being in a bad state.
                await conversationState.DeleteAsync(turnContext).ConfigureAwait(false);
            };
        }
    }
}
