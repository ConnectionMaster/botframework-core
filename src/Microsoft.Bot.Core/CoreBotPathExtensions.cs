// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Core
{
    /*
     * TODO: Refactor bot path adapter
     * BODY: Runtime should consume bot path value through application setting, per environment.
     */
    /// <summary>
    /// Bot path adapter, for development environment, use '../../' as the bot path, for deployment and production environment, use 'ComposerDialogs' as bot path
    /// </summary>
    public static class CoreBotPathExtensions
    {
        public static IConfigurationBuilder UseBotPathConverter(this IConfigurationBuilder builder, bool isDevelopment = true)
        {
            var settings = new Dictionary<string, string>();
            if (isDevelopment)
            {
                settings["bot"] = "../../";
            }
            else
            {
                settings["bot"] = "ComposerDialogs";
            }
            builder.AddInMemoryCollection(settings);
            return builder;
        }
    }
}
