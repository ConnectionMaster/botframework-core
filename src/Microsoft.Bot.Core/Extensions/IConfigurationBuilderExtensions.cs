﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Core.Extensions
{
    /*
     * TODO: Refactor bot path adapter
     * BODY: Runtime should consume bot path value through application setting, per environment.
     */

    /// <summary>
    /// Bot path adapter, for development environment, use '../../' as the bot path, for deployment and production environment, use 'ComposerDialogs' as bot path.
    /// </summary>
    public static class IConfigurationBuilderExtensions
    {
        private const string ComposerDialogsDirectoryName = "ComposerDialogs";
        private const string DevelopmentApplicationRoot = "../../../";
        private const string DialogFileExtension = ".dialog";

        public static IConfigurationBuilder AddBotCoreConfiguration(
            this IConfigurationBuilder builder,
            string applicationRoot,
            bool isDevelopment = true)
        {
            if (string.IsNullOrEmpty(applicationRoot))
            {
                throw new ArgumentNullException(nameof(applicationRoot));
            }

            applicationRoot = isDevelopment ? DevelopmentApplicationRoot : applicationRoot;

            string botRoot = isDevelopment
                ? applicationRoot
                : Path.Combine(applicationRoot, ComposerDialogsDirectoryName);

            var settings = new Dictionary<string, string>
            {
                {
                    ConfigurationConstants.ApplicationRootKey,
                    applicationRoot
                },
                {
                    ConfigurationConstants.BotKey,
                    botRoot
                },
                {
                    ConfigurationConstants.DefaultRootDialogKey,
                    GetDefaultRootDialog(botRoot)
                }
            };

            builder.AddInMemoryCollection(settings);
            return builder;
        }

        /*
         * TODO: Refactor the luis and qnamaker Composer-based settings extensions adapter
         * BODY: This method utilizes the settings file generated by bf luis:build and qna:build commands, aligning to Composer-specific settings. 
         */

        /// <summary>
        /// Setup configuration to utilize the settings file generated by bf luis:build and qna:build. This is a luis and qnamaker settings extensions adapter aligning with Composer customized settings.
        /// </summary>
        /// <remarks>
        /// This will pick up LUIS_AUTHORING_REGION or --region settings as the setting to target.
        /// This will pick up --environment as the environment to target.  If environment is Development it will use the name of the logged in user.
        /// This will pick up --root as the root folder to run in.
        /// </remarks>
        /// <param name="builder">Configuration builder to modify.</param>
        /// <returns>Modified configuration builder.</returns>
        public static IConfigurationBuilder AddComposerConfiguration(this IConfigurationBuilder builder)
        {
            var configuration = builder.Build();
            var botRoot = configuration.GetValue<string>("bot") ?? ".";
            var luisRegion = configuration.GetValue<string>("LUIS_AUTHORING_REGION") ?? configuration.GetValue<string>("luis:authoringRegion") ?? configuration.GetValue<string>("luis:region") ?? "westus";
            var qnaRegion = configuration.GetValue<string>("qna:qnaRegion") ?? "westus";
            var environment = configuration.GetValue<string>("luis:environment") ?? Environment.UserName;
            var settings = new Dictionary<string, string>();
            var luisEndpoint = configuration.GetValue<string>("luis:endpoint");
            if (string.IsNullOrWhiteSpace(luisEndpoint))
            {
                luisEndpoint = $"https://{luisRegion}.api.cognitive.microsoft.com";
            }

            settings["luis:endpoint"] = luisEndpoint;
            settings["BotRoot"] = botRoot;
            builder.AddInMemoryCollection(settings);
            if (environment == "Development")
            {
                environment = Environment.UserName;
            }

            var luisSettingsPath = Path.GetFullPath(Path.Combine(botRoot, "generated", $"luis.settings.{environment.ToLowerInvariant()}.{luisRegion}.json"));
            var luisSettingsFile = new FileInfo(luisSettingsPath);
            if (luisSettingsFile.Exists)
            {
                builder.AddJsonFile(luisSettingsFile.FullName, optional: false, reloadOnChange: true);
            }

            var qnaSettingsPath = Path.GetFullPath(Path.Combine(botRoot, "generated", $"qnamaker.settings.{environment.ToLowerInvariant()}.{qnaRegion}.json"));
            var qnaSettingsFile = new FileInfo(qnaSettingsPath);
            if (qnaSettingsFile.Exists)
            {
                builder.AddJsonFile(qnaSettingsFile.FullName, optional: false, reloadOnChange: true);
            }

            return builder;
        }

        private static string GetDefaultRootDialog(string botRoot)
        {
            var directory = new DirectoryInfo(botRoot);
            foreach (FileInfo file in directory.GetFiles())
            {
                if (string.Equals(DialogFileExtension, file.Extension, StringComparison.OrdinalIgnoreCase))
                {
                    return file.Name;
                }
            }

            return null;
        }
    }
}
