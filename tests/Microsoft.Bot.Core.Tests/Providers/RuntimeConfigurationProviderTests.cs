﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using AdaptiveExpressions.Properties;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Generators;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Core.Extensions;
using Microsoft.Bot.Core.Providers;
using Microsoft.Bot.Core.Providers.Adapter;
using Microsoft.Bot.Core.Providers.Channel;
using Microsoft.Bot.Core.Providers.Credentials;
using Microsoft.Bot.Core.Providers.Storage;
using Microsoft.Bot.Core.Providers.Telemetry;
using Microsoft.Bot.Core.Settings;
using Microsoft.Bot.Core.Tests.Resources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Xunit;
using IChannelProvider = Microsoft.Bot.Core.Providers.Channel.IChannelProvider;
using ICredentialProvider= Microsoft.Bot.Core.Providers.Credentials.ICredentialProvider;

namespace Microsoft.Bot.Core.Tests.Providers
{
    [Collection("ComponentRegistrations")]
    public class RuntimeConfigurationProviderTests
    {
        private const string DialogId = "TestRoot";
        private const string ResourceId = "root.dialog";

        public static IEnumerable<object[]> GetConfigureServicesSucceedsData()
        {
            yield return new object[]
            {
                new List<IAdapterProvider> { new BotCoreAdapterProvider() },
                (IChannelProvider)null,
                new DeclarativeCredentialsProvider(),
                (string)null,
                false,
                new StringExpression(ResourceId),
                new MemoryStorageProvider(),
                (ITelemetryProvider)null,
                TestDataGenerator.BuildConfigurationRoot()
            };

            yield return new object[]
            {
                new List<IAdapterProvider> { new BotCoreAdapterProvider() },
                new DeclarativeChannelProvider(),
                new DeclarativeCredentialsProvider(),
                "en-CA",
                true,
                new StringExpression("=rootDialog"),
                new MemoryStorageProvider(),
                new ApplicationInsightsTelemetryProvider(),
                TestDataGenerator.BuildConfigurationRoot(new JObject
                {
                    { "rootDialog", ResourceId }
                })
            };
        }

        [Theory]
        [MemberData(nameof(GetConfigureServicesSucceedsData))]
        public void ConfigureServices_Succeeds(
            IList<IAdapterProvider> adapters,
            IChannelProvider channel,
            ICredentialProvider credential,
            string defaultLocale,
            bool removeRecipientMention,
            StringExpression rootDialog,
            IStorageProvider storage,
            ITelemetryProvider telemetry,
            IConfiguration configuration)
        {
            var services = new ServiceCollection();

            services.AddTransient<IConfiguration>(_ => configuration);
            services.AddTransient<IHostingEnvironment, HostingEnvironment>();
            services.AddTransient<ResourceExplorer>(_ => TestDataGenerator.BuildMemoryResourceExplorer(new[]
            {
                new JsonResource(ResourceId, data: BuildDialog())
            }));

            var runtimeConfigurationProvider = new RuntimeConfigurationProvider
            {
                Channel = channel,
                Credentials = credential,
                DefaultLocale = defaultLocale,
                RemoveRecipientMention = removeRecipientMention,
                RootDialog = rootDialog,
                Storage = storage,
                Telemetry = telemetry
            };

            foreach (IAdapterProvider adapter in adapters)
            {
                runtimeConfigurationProvider.Adapters.Add(adapter);
            }

            runtimeConfigurationProvider.ConfigureServices(services, configuration);

            IServiceProvider provider = services.BuildServiceProvider();

            Assertions.AssertService<AuthenticationConfiguration>(
                services,
                provider,
                ServiceLifetime.Singleton);

            Assertions.AssertService<ConversationState>(
                services,
                provider,
                ServiceLifetime.Singleton);

            Assertions.AssertService<UserState>(
                services,
                provider,
                ServiceLifetime.Singleton);

            Assertions.AssertService<SkillConversationIdFactoryBase, SkillConversationIdFactory>(
                services,
                provider,
                ServiceLifetime.Singleton);

            Assertions.AssertOptions<CoreBotOptions>(
                provider,
                assert: (options) =>
                {
                    Assert.Equal(expected: defaultLocale, actual: options.DefaultLocale);
                    Assert.Equal(expected: ResourceId, actual: options.RootDialog);
                    Assert.Equal(expected: removeRecipientMention, actual: options.RemoveRecipientMention);
                });

            Assertions.AssertService<IBot, CoreBot>(
                services,
                provider,
                ServiceLifetime.Singleton);

            Assertions.AssertService<ChannelServiceHandler, SkillHandler>(
                services,
                provider,
                ServiceLifetime.Singleton);
        }

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
                () => new RuntimeConfigurationProvider().ConfigureServices(services, configuration));
        }

        [Fact]
        public void ConfigureServices_Throws_ResourceExplorerRequired()
        {
            var services = new ServiceCollection();
            IConfiguration configuration = TestDataGenerator.BuildConfigurationRoot();

            services.AddTransient<IConfiguration>(_ => configuration);

            new RuntimeConfigurationProvider
            {
                Adapters = { new BotCoreAdapterProvider() },
                Credentials = new DeclarativeCredentialsProvider(),
                RemoveRecipientMention = true,
                Storage = new MemoryStorageProvider()
            }.ConfigureServices(services, configuration);

            IServiceProvider provider = services.BuildServiceProvider();

            Assertions.AssertService<AuthenticationConfiguration>(
                services,
                provider,
                ServiceLifetime.Singleton);

            Assertions.AssertService<ConversationState>(
                services,
                provider,
                ServiceLifetime.Singleton);

            Assertions.AssertService<UserState>(
                services,
                provider,
                ServiceLifetime.Singleton);

            Assertions.AssertService<SkillConversationIdFactoryBase, SkillConversationIdFactory>(
                services,
                provider,
                ServiceLifetime.Singleton);

            Assertions.AssertOptions<CoreBotOptions>(
                provider,
                assert: (options) =>
                {
                    Assert.Null(options.DefaultLocale);
                    Assert.Null(options.RootDialog);
                    Assert.True(options.RemoveRecipientMention);
                });

            Assertions.AssertServiceThrows<IBot, CoreBot, InvalidOperationException>(
                services,
                provider,
                ServiceLifetime.Singleton,
                assert: (exception) =>
                {
                    Assert.Equal(
                        expected: $"No service for type '{typeof(ResourceExplorer).FullName}' has been registered.",
                        actual: exception.Message);
                });
        }

        [Fact]
        public void ConfigureServices_Throws_RootDialogNotFound()
        {
            var services = new ServiceCollection();
            IConfiguration configuration = TestDataGenerator.BuildConfigurationRoot();
            services.AddTransient<ResourceExplorer>(_ => TestDataGenerator.BuildMemoryResourceExplorer());

            services.AddTransient<IConfiguration>(_ => configuration);

            new RuntimeConfigurationProvider
            {
                Adapters = { new BotCoreAdapterProvider() },
                Credentials = new DeclarativeCredentialsProvider(),
                RemoveRecipientMention = true,
                RootDialog = ResourceId,
                Storage = new MemoryStorageProvider()
            }.ConfigureServices(services, configuration);

            IServiceProvider provider = services.BuildServiceProvider();

            Assertions.AssertService<AuthenticationConfiguration>(
                services,
                provider,
                ServiceLifetime.Singleton);

            Assertions.AssertService<ConversationState>(
                services,
                provider,
                ServiceLifetime.Singleton);

            Assertions.AssertService<UserState>(
                services,
                provider,
                ServiceLifetime.Singleton);

            Assertions.AssertService<SkillConversationIdFactoryBase, SkillConversationIdFactory>(
                services,
                provider,
                ServiceLifetime.Singleton);

            Assertions.AssertOptions<CoreBotOptions>(
                provider,
                assert: (options) =>
                {
                    Assert.Null(options.DefaultLocale);
                    Assert.Equal(expected: ResourceId, actual: options.RootDialog);
                    Assert.True(options.RemoveRecipientMention);
                });

            Assertions.AssertServiceThrows<IBot, CoreBot, ArgumentException>(
                services,
                provider,
                ServiceLifetime.Singleton,
                assert: (exception) =>
                {
                    Assert.StartsWith(
                        expectedStartString: $"Could not find resource '{ResourceId}'",
                        actualString: exception.Message);

                    Assert.Equal(expected: ResourceId, actual: exception.ParamName);
                });
        }

        private static AdaptiveDialog BuildDialog(string dialogId = null)
        {
            return new AdaptiveDialog(dialogId ?? DialogId)
            {
                Generator = new TemplateEngineLanguageGenerator(),
                Recognizer = new RegexRecognizer(),
                Triggers =
                {
                    new OnUnknownIntent(
                        actions: new List<Dialog>
                        {
                            new SendActivity("Hello World!")
                        })
                }
            };
        }
    }
}
