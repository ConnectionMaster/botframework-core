// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using AdaptiveExpressions.Properties;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Microsoft.Bot.Core.Providers.TranscriptStore
{
    [JsonObject]
    public class BlobsTranscriptStoreProvider : ITranscriptStoreProvider
    {
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.BlobsTranscriptStore";

        [JsonProperty("connectionString")]
        public StringExpression ConnectionString { get; set; }

        [JsonProperty("containerName")]
        public StringExpression ContainerName { get; set; }

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            if (services == null) { throw new ArgumentNullException(nameof(services)); }
            if (configuration == null) { throw new ArgumentNullException(nameof(configuration)); }
        }
    }
}
