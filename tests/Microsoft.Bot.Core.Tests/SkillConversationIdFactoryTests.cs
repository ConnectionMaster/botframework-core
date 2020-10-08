// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Skills;
using Microsoft.Bot.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Core.Tests
{
    [TestClass]
    public class SkillConversationIdFactoryTests
    {
        private const string ServiceUrl = "http://testbot.com/api/messages";
        private const string SkillId = "skill";

        private string ApplicationId { get; } = Guid.NewGuid().ToString(format: "N");

        private string BotId { get; } = Guid.NewGuid().ToString(format: "N");

        private SkillConversationIdFactory SkillConversationIdFactory { get; } =
            new SkillConversationIdFactory(new MemoryStorage());

        private static void AssertConversationReference(ConversationReference expected, ConversationReference actual)
        {
            if (expected == null) { throw new ArgumentNullException(nameof(expected)); }
            if (actual == null) { throw new ArgumentNullException(nameof(actual)); }

            Assert.IsNotNull(expected.Conversation, "Expected Conversation");
            Assert.IsNotNull(actual.Conversation, "Actual Conversation");

            Assert.AreEqual(expected.Conversation.Id, actual.Conversation.Id, "Conversation.Id");
            Assert.AreEqual(expected.ServiceUrl, actual.ServiceUrl, "ServiceUrl");
        }

        private BotFrameworkSkill BuildBotFrameworkSkill()
        {
            return new BotFrameworkSkill
            {
                AppId = this.ApplicationId,
                Id = SkillId,
                SkillEndpoint = new Uri(ServiceUrl)
            };
        }

        private static ConversationReference BuildConversationReference()
        {
            return new ConversationReference
            {
                Conversation = new ConversationAccount(id: Guid.NewGuid().ToString("N")),
                ServiceUrl = ServiceUrl
            };
        }

        private static Activity BuildMessageActivity(ConversationReference conversationReference)
        {
            if (conversationReference == null) { throw new ArgumentNullException(nameof(conversationReference)); }

            var activity = (Activity)Activity.CreateMessageActivity();
            activity.ApplyConversationReference(conversationReference);

            return activity;
        }

        [TestMethod]
        public async Task SkillConversationIdFactoryHappyPath()
        {
            ConversationReference conversationReference = BuildConversationReference();

            string skillConversationId = await this.SkillConversationIdFactory.CreateSkillConversationIdAsync(
                options: new SkillConversationIdFactoryOptions
                {
                    Activity = BuildMessageActivity(conversationReference),
                    BotFrameworkSkill = this.BuildBotFrameworkSkill(),
                    FromBotId = this.BotId,
                    FromBotOAuthScope = this.BotId,
                },
                cancellationToken: CancellationToken.None);

            Assert.IsFalse(
                string.IsNullOrEmpty(skillConversationId),
                "Expected a valid skill conversation ID to be created");

            SkillConversationReference skillConversationReference =
                await this.SkillConversationIdFactory.GetSkillConversationReferenceAsync(
                    skillConversationId,
                    CancellationToken.None);

            AssertConversationReference(conversationReference, skillConversationReference.ConversationReference);

            await this.SkillConversationIdFactory.DeleteConversationReferenceAsync(
                skillConversationId,
                CancellationToken.None);

            skillConversationReference = await this.SkillConversationIdFactory.GetSkillConversationReferenceAsync(
                skillConversationId,
                CancellationToken.None);

            Assert.IsNull(
                skillConversationReference,
                "Expected skill conversation reference to be null following deletion");
        }
    }
}
