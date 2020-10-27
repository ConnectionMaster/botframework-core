// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Core.Builders.Middleware;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Core.Tests.Builders.Middleware
{
    public class RemoveRecipientMentionMiddlewareBuilderTests
    {
        [Fact]
        public void Build_Succeeds()
        {
            IServiceProvider services = new ServiceCollection()
                .BuildServiceProvider();

            IConfiguration configuration = TestDataGenerator.BuildConfigurationRoot();

            IMiddleware middleware = new RemoveRecipientMentionMiddlewareBuilder().Build(services, configuration);

            Assert.NotNull(middleware);
            Assert.IsType<RemoveRecipientMentionMiddlewareBuilder>(middleware);
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
                () => new RemoveRecipientMentionMiddlewareBuilder().Build(services, configuration));
        }

        /// <summary>
        ///  Slack uses @username and is expected in the Mention.text property.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task RemoveSlackAtMention()
        {
            var adapter = new TestAdapter(TestAdapter.CreateConversation("RemoveSlackAtMention"))
                .Use(new RemoveRecipientMentionMiddlewareBuilder());

            // Mock Message Activity with mention properties
            var mentionProperties = JObject.Parse(
                @"{
                  ""mentioned"": {
                            ""id"": ""bot"",
                    ""name"": ""Bot""
                  },
                  ""text"": ""@Bot"",
                  ""type"": ""mention""
                }"
            );

            var mention = new Mention
            {
                Mentioned = adapter.Conversation.Bot,
                Text = $"@{adapter.Conversation.Bot.Name}",
                Properties = mentionProperties
            };

            var mentionActivity = MessageFactory.Text($"{mention.Text} Hi Bot");
            mentionActivity.Entities = new List<Entity> { mention };

            await new TestFlow(adapter, async (context, cancellationToken) =>
            {
                Assert.Equal("Hi Bot", context.Activity.Text);

                await Task.CompletedTask;
            })
                .Send(mentionActivity)
                .StartTestAsync();
        }

        /// <summary>
        /// Teams uses &lt;at&gt;username&lt;/at&gt; and is expected in the Mention.text property.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task RemoveTeamsAtMention()
        {
            var adapter = new TestAdapter(TestAdapter.CreateConversation("RemoveTeamsAtMention"))
                .Use(new RemoveRecipientMentionMiddlewareBuilder());

            // Mock Message Activity with mention properties
            var mentionProperties = JObject.Parse(
                @"{
                  ""mentioned"": {
                            ""id"": ""bot"",
                    ""name"": ""Bot""
                  },
                  ""text"": ""<at>Bot</at>"",
                  ""type"": ""mention""
                }"
            );

            var mention = new Mention
            {
                Mentioned = adapter.Conversation.Bot,
                Text = $"<at>{adapter.Conversation.Bot.Name}</at>",
                Properties = mentionProperties
            };

            var mentionActivity = MessageFactory.Text($"{mention.Text} Hi Bot");
            mentionActivity.Entities = new List<Entity> { mention };

            await new TestFlow(adapter, async (context, cancellationToken) =>
            {
                Assert.Equal("Hi Bot", context.Activity.Text);

                await Task.CompletedTask;
            })
                .Send(mentionActivity)
                .StartTestAsync();
        }
    }
}
