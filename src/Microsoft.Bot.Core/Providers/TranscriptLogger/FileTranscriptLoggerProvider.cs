// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using AdaptiveExpressions.Properties;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Microsoft.Bot.Core.Providers.TranscriptLogger
{
    [JsonObject]
    public class FileTranscriptLoggerProvider : ITranscriptLoggerProvider
    {
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.FileTranscriptLogger";

        [JsonProperty("folder")]
        public StringExpression Folder { get; set; }

        [JsonProperty("unitTestMode")]
        public BoolExpression UnitTestMode { get; set; }

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            if (services == null) { throw new ArgumentNullException(nameof(services)); }
            if (configuration == null) { throw new ArgumentNullException(nameof(configuration)); }
        }
    }
}
