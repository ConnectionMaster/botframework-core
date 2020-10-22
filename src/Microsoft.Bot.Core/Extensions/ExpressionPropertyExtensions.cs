﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using AdaptiveExpressions.Properties;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Bot.Core.Extensions
{
    public static class ExpressionPropertyExtensions
    {
        public static T GetConfigurationValue<T>(this ExpressionProperty<T> property, IConfiguration configuration)
        {
            if (property == null) { throw new ArgumentNullException(nameof(property)); }
            if (configuration == null) { throw new ArgumentNullException(nameof(configuration)); }

            return property.GetValue(new AdaptiveConfiguration(configuration));
        }
    }
}
