// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using Xunit;

namespace Microsoft.Bot.Core.Tests.Fixtures
{
    public class ComponentRegistrationsFixture : IDisposable
    {
        public ComponentRegistrationsFixture()
        {
            ComponentRegistrations.Add();
        }

        public void Dispose()
        {
        }
    }

    [CollectionDefinition("ComponentRegistrations")]
    public class ComponentRegistrationsCollection : ICollectionFixture<ComponentRegistrationsFixture>
    {
    }
}
