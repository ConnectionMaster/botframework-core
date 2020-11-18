// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Core.Builders.Middleware;
using Microsoft.Bot.Core.Builders.OnTurnError;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Core.Tests.Builders.OnTurnError
{
    public class OnTurnErrorBuilderTests
    {
        public static IEnumerable<object[]> GetBuildSucceedsData()
        {
            yield return new object[]
            {
                (BoolExpression)null,
                (BoolExpression)null,
                (ILogger<BotFrameworkHttpAdapter>)null,
                (IConfiguration)TestDataGenerator.BuildConfigurationRoot()
            };

            yield return new object[]
            {
                new BoolExpression(true),
                new BoolExpression(true),
                (ILogger<BotFrameworkHttpAdapter>)null,
                (IConfiguration)TestDataGenerator.BuildConfigurationRoot()
            };

            yield return new object[]
            {
                new BoolExpression("=logError"),
                new BoolExpression("=sendTraceActivities"),
                (ILogger<BotFrameworkHttpAdapter>)null,
                (IConfiguration)TestDataGenerator.BuildConfigurationRoot(new JObject
                {
                    { "logError", true },
                    { "sendTraceActivities", true }
                })
            };
        }

        [Theory]
        [MemberData(nameof(GetBuildSucceedsData))]
        public void Build_Succeeds(
            BoolExpression logError,
            BoolExpression sendTraceActivity,
            ILogger<BotFrameworkHttpAdapter> logger,
            IConfiguration configuration)
        {
            IServiceProvider services = new ServiceCollection()
                .AddTransient<ILogger>(_ => logger)
                .AddTransient<BotState, ConversationState>()
                .BuildServiceProvider();

            Func<ITurnContext, Exception, Task> onTurnError = new OnTurnErrorBuilder
            {
                LogError = logError,
                SendTraceActivity = sendTraceActivity
            }.Build(services, configuration);

            Assert.NotNull(onTurnError);
            Assert.IsType<Func<ITurnContext, Exception, Task>>(onTurnError);
        }

        [Theory]
        [MemberData(
            nameof(BuilderTestDataGenerator.GetBuildArgumentNullExceptionData),
            MemberType = typeof(BuilderTestDataGenerator))]
        public void Build_Throws_ArgumentNullException(
            string paramName,
            IServiceProvider services,
            IConfiguration configuration)
        {
            Assert.Throws<ArgumentNullException>(
                paramName,
                () => new OnTurnErrorBuilder().Build(services, configuration));
        }
    }
}
