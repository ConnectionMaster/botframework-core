using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Core.Middleware
{
    /// <summary>
    /// When added, this middleware will check if incoming activities are of Message 
    /// type and if so, remove recipent mentions from the Text property.
    /// </summary>
    public class RemoveRecipientMiddleware : IMiddleware
    {
        /// <summary>
        /// Processes an incoming activity.
        /// </summary>
        /// <param name="turnContext">The context object for this turn.</param>
        /// <param name="next">The delegate to call to continue the bot middleware pipeline.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>Spawns a thread that sends the periodic typing activities until the turn ends.
        /// </remarks>
        /// <seealso cref="ITurnContext"/>
        /// <seealso cref="Bot.Schema.IActivity"/>
        public async Task OnTurnAsync(ITurnContext turnContext, NextDelegate next, CancellationToken cancellationToken)
        {
            if (turnContext?.Activity?.Type == ActivityTypes.Message)
            {
                turnContext.Activity.RemoveRecipientMention();
            }

            await next(cancellationToken).ConfigureAwait(false);
        }
    }
}