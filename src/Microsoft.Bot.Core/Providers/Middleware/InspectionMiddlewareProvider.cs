// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Microsoft.Bot.Core.Providers.Middleware
{
    [JsonObject]
    public class InspectionMiddlewareProvider : IMiddlewareProvider
    {
        [JsonProperty("$kind")]
        public const string Kind = "Microsoft.InspectionMiddleware";

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            if (services == null) { throw new ArgumentNullException(nameof(services)); }
            if (configuration == null) { throw new ArgumentNullException(nameof(configuration)); }
        }
    }
}
