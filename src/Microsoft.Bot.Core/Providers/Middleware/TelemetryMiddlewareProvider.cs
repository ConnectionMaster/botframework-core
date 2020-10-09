// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using AdaptiveExpressions.Properties;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Microsoft.Bot.Core.Providers.Middleware
{
    [JsonObject]
    public class TelemetryMiddlewareProvider : IMiddlewareProvider
    {
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.TelemetryMiddleware";

        [JsonProperty("logActivities")]
        public BoolExpression LogActivities { get; set; }

        [JsonProperty("logPersonalInformation")]
        public BoolExpression LogPersonalInformation { get; set; }

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            if (services == null) { throw new ArgumentNullException(nameof(services)); }
            if (configuration == null) { throw new ArgumentNullException(nameof(configuration)); }
        }
    }
}
