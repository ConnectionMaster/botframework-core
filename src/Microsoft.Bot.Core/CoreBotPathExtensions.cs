// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
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
        private const string ComposerDialogsDirectoryName = "ComposerDialogs";
        private const string DevelopmentApplicationRoot = "../../../";

        public static IConfigurationBuilder UseBotPathConverter(
            this IConfigurationBuilder builder,
            string applicationRoot,
            bool isDevelopment = true)
        {
            if (string.IsNullOrEmpty(applicationRoot)) { throw new ArgumentNullException(nameof(applicationRoot)); }

            applicationRoot = isDevelopment ? DevelopmentApplicationRoot : applicationRoot;

            var settings = new Dictionary<string, string>
            {
                {
                    ConfigurationConstants.ApplicationRootKey,
                    applicationRoot
                },
                {
                    ConfigurationConstants.BotKey,
                    isDevelopment ? applicationRoot : Path.Combine(applicationRoot, ComposerDialogsDirectoryName)
                }
            };

            builder.AddInMemoryCollection(settings);
            return builder;
        }
    }
}
