// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Bot.Builder.Azure;

namespace Microsoft.Bot.Core.Settings
{
    public class BotSettings
    {
        public BotFeatureSettings Feature { get; set; }

        public BlobStorageConfiguration BlobStorage { get; set; }

        public string MicrosoftAppId { get; set; }

        public string MicrosoftAppPassword { get; set; }

        public CosmosDbPartitionedStorageOptions CosmosDb { get; set; }

        public TelemetryConfiguration ApplicationInsights { get; set; }

        public AdditionalTelemetryConfiguration Telemetry { get; set; }

        public string Bot { get; set; }

        public static bool ConfigSectionValid(string val)
        {
            return !string.IsNullOrEmpty(val) && !val.StartsWith("<", StringComparison.Ordinal);
        }
    }
}
