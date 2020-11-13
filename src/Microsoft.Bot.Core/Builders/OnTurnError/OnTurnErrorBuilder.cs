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

        private const bool DefaultLogError = false;

        private const bool DefaultSendTraceActivity = false;

        private const string DefaultLogErrorMessage = "Exception caught:";

        private Templates _templates;

        /// <summary>
        /// Gets or sets the boolean flag to log errors. Defaults to false.
        /// </summary>
        [JsonProperty("logError")]
        private BoolExpression LogError { get; set; }

        /// <summary>
        /// Gets or sets the boolean flag to send a trace activity with the stack trace to the user. Defaults to false.
        /// </summary>
        [JsonProperty("sendTraceActivity")]
        private BoolExpression SendTraceActivity { get; set; }

        /// <summary>
        /// Gets or sets the LG file containing related templates.
        /// </summary>
        [JsonProperty("lgFile")]
        private StringExpression LGFile { get; set; }

        /// <summary>
        /// Gets or sets the LG template for the response to the user. The template can reference the Exception object's properties.
        /// </summary>
        [JsonProperty("responseLgTemplate")]
        private StringExpression ResponseTemplate { get; set; }

        /// <summary>
        /// Gets or sets the LG template for the exception logged message. The template can reference the Exception object's properties.
        /// </summary>
        [JsonProperty("logErrorMessageLgTemplate")]
        private StringExpression LogErrorMessageTemplate { get; set; }

        public Func<ITurnContext, Exception, Task> Build(IServiceProvider services, IConfiguration configuration, ILogger<IBotFrameworkHttpAdapter> logger)
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
                // Get path of provided LG file
                string[] paths = { "..", "..", "..", "language-generation", turnContext.Activity.Locale, $"{this.LGFile?.GetConfigurationValue(configuration)}.{turnContext.Activity.Locale}.lg" };

                _templates = Templates.ParseFile(Path.Combine(paths));

                // Log any leaked exception from the application.
                if (this.LogError?.GetConfigurationValue(configuration) ?? DefaultLogError)
                {
                    logger.LogError(exception, $"{_templates.Evaluate(this.LogErrorMessageTemplate?.GetConfigurationValue(configuration), exception)}" ?? $"{DefaultLogErrorMessage} {exception.Message}");
                }

                // Send a catch-all apology to the user.
                await turnContext.SendActivityAsync(ActivityFactory.FromObject(_templates.Evaluate(this.ResponseTemplate?.GetConfigurationValue(configuration), exception))).ConfigureAwait(false);

                // Send a trace activity with the exception to the user.
                if (this.SendTraceActivity?.GetConfigurationValue(configuration) ?? DefaultSendTraceActivity)
                {
                    await turnContext.SendActivityAsync(new Activity(type: ActivityTypes.Trace, text: exception.StackTrace)).ConfigureAwait(false);
                }

                // Delete the conversationState for the current conversation to prevent the
                // bot from getting stuck in a error-loop caused by being in a bad state.
                await conversationState.DeleteAsync(turnContext).ConfigureAwait(false);
            };
        }

        public Func<ITurnContext, Exception, Task> Build(IServiceProvider services, IConfiguration configuration)
        {
            throw new NotImplementedException();
        }
    }
}
