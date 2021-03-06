﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Builder.Runtime.Builders.Transcripts
{
    /// <summary>
    /// Defines an interface for an implementation of <see cref="IBuilder{T}"/> that returns an
    /// instance whose type implements <see cref="ITranscriptLogger"/>.
    /// </summary>
    internal interface ITranscriptLoggerBuilder : IBuilder<ITranscriptLogger>
    {
    }
}
