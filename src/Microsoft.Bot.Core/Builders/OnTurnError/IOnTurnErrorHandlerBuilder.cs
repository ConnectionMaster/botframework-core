// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Core.Providers;

namespace Microsoft.Bot.Core.Builders.OnTurnError
{
    public interface IOnTurnErrorHandlerBuilder : IBuilder<Func<ITurnContext, Exception, Task>>
    {
    }
}
