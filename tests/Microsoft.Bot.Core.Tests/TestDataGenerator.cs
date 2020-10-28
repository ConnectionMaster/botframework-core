﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Core.Tests.Resources;
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

        public static ResourceExplorer BuildMemoryResourceExplorer(IEnumerable<MemoryResource> resources = null)
        {
            var resourceExplorer = new ResourceExplorer();
            var resourceProvider = new MemoryResourceProvider(
                resourceExplorer,
                resources ?? Array.Empty<MemoryResource>());

            resourceExplorer.AddResourceProvider(resourceProvider);
            resourceExplorer.RegisterType<OnQnAMatch>(OnQnAMatch.Kind);

            return resourceExplorer;
        }
    }
}
