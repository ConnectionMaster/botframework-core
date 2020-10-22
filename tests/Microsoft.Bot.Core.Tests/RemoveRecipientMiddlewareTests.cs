// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Core.Builders.Middleware;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Bot.Core.Tests
{
    public class RemoveRecipientMiddlewareTests
    {
        /// <summary>
        ///  Slack uses @username and is expected in the Mention.text property.
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task RemoveSlackAtMention()
        {
            var adapter = new TestAdapter(TestAdapter.CreateConversation("RemoveAtMention"))
                .Use(new RemoveRecipientMiddlewareBuilder());

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
            var adapter = new TestAdapter(TestAdapter.CreateConversation("RemoveAtMention"))
                .Use(new RemoveRecipientMiddlewareBuilder());

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
            })
                .Send(mentionActivity)
                .StartTestAsync();
        }
    }
}
