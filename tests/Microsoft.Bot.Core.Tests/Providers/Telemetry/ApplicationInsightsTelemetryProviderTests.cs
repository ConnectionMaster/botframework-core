﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Core.Providers.Telemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Bot.Core.Tests.Providers.Telemetry
{
    public class ApplicationInsightsTelemetryProviderTests
    {
        [Theory]
        [MemberData(
            nameof(ProviderTestDataGenerator.GetConfigureServicesArgumentNullExceptionData),
            MemberType = typeof(ProviderTestDataGenerator))]
        public void ConfigureServices_Throws_ArgumentNullException(
            string paramName,
            IServiceCollection services,
            IConfiguration configuration)
        {
            Assert.Throws<ArgumentNullException>(
                paramName,
                () => new ApplicationInsightsTelemetryProvider().ConfigureServices(services, configuration));
        }
    }
}
