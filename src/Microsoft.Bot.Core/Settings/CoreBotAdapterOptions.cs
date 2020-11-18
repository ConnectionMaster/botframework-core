// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Core.Builders.Middleware;
using Microsoft.Bot.Core.Builders.OnTurnError;

namespace Microsoft.Bot.Core.Settings
{
    public class CoreBotAdapterOptions
    {
        public IList<IMiddlewareBuilder> Middleware { get; } = new List<IMiddlewareBuilder>();

        public IOnTurnErrorBuilder OnTurnError { get; set; }
    }
}
