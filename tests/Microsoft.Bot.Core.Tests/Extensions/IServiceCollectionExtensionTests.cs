// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Core.Extensions;
using Microsoft.Bot.Core.Providers;
using Microsoft.Bot.Core.Providers.Adapter;
using Microsoft.Bot.Core.Providers.Credentials;
using Microsoft.Bot.Core.Providers.Storage;
using Microsoft.Bot.Core.Tests.Resources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Bot.Core.Tests.Extensions
{
    [Collection("ComponentRegistrations")]
    public class IServiceCollectionExtensionTests
    {
        private const string ResourceId = "runtime.json";

        public static IEnumerable<object[]> GetAddBotCoreThrowsArgumentNullExceptionData()
        {
            IServiceCollection services = new ServiceCollection();
            IConfiguration configuration = TestDataGenerator.BuildConfigurationRoot();
            ResourceExplorer resourceExplorer = TestDataGenerator.BuildMemoryResourceExplorer();

            yield return new object[]
            {
                "services",
                (IServiceCollection)null,
                configuration,
                resourceExplorer
            };

            yield return new object[]
            {
                "configuration",
                services,
                (IConfiguration)null,
                resourceExplorer
            };

            yield return new object[]
            {
                "resourceExplorer",
                services,
                configuration,
                (ResourceExplorer)null
            };
        }

        [Fact]
        public void AddBotCore_Succeeds()
        {
            IServiceCollection services = new ServiceCollection();
            IConfiguration configuration = TestDataGenerator.BuildConfigurationRoot();
            ResourceExplorer resourceExplorer = TestDataGenerator.BuildMemoryResourceExplorer(new[]
            {
                new JsonResource(ResourceId, new RuntimeConfigurationProvider
                {
                    Adapters = { new BotCoreAdapterProvider() },
                    Credentials = new DeclarativeCredentialsProvider(),
                    RemoveRecipientMention = true,
                    RootDialog = "root.dialog",
                    Storage = new MemoryStorageProvider()
                })
            });

            services.AddBotCore(configuration, resourceExplorer);
        }

        [Theory]
        [MemberData(nameof(GetAddBotCoreThrowsArgumentNullExceptionData))]
        public void AddBotCore_Throws_ArgumentNullException(
            string paramName,
            IServiceCollection services,
            IConfiguration configuration,
            ResourceExplorer resourceExplorer)
        {
            Assert.Throws<ArgumentNullException>(
                paramName,
                () => services.AddBotCore(configuration, resourceExplorer));
        }

        [Fact]
        public void AddBotCore_Throws_RuntimeConfigurationNotFound()
        {
            IServiceCollection services = new ServiceCollection();
            IConfiguration configuration = TestDataGenerator.BuildConfigurationRoot();
            ResourceExplorer resourceExplorer = TestDataGenerator.BuildMemoryResourceExplorer();

            ArgumentException exception = Assert.Throws<ArgumentException>(
                () => services.AddBotCore(configuration, resourceExplorer));

            Assert.StartsWith(
                expectedStartString: $"Could not find resource '{ResourceId}'",
                actualString: exception.Message);

            Assert.Equal(expected: ResourceId, actual: exception.ParamName);
        }
    }
}
