// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Core.Settings
{
    public class BotFeatureSettings
    {
        public bool UseShowTypingMiddleware { get; set; }

        public bool UseInspectionMiddleware { get; set; }

        /// <summary>
        /// Use RemoveRecipientMention Activity Extensions
        /// </summary>
        public bool RemoveRecipientMention { get; set; }
    }
}
