// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Core.Tests
{
    public class TestDataGenerator
    {
        public static IConfigurationRoot BuildConfigurationRoot(JObject jObject = null)
        {
            return new ConfigurationBuilder()
                .Add(new JObjectConfigurationSource(jObject ?? new JObject()))
                .Build();
        }
    }
}
