// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Provides an API point for provider-specific extensions for configuring a <see cref="ITrigger" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-triggers">Database triggers</see> for more information and examples.
/// </remarks>
public class TriggerBuilder
{
    /// <summary>
    ///     Creates a new builder for the given <see cref="ITrigger" />.
    /// </summary>
    /// <param name="trigger">The <see cref="IMutableTrigger" /> to configure.</param>
    public TriggerBuilder(IMutableTrigger trigger)
        => Metadata = trigger;

    /// <summary>
    ///     The trigger.
    /// </summary>
    public virtual IMutableTrigger Metadata { get; }
}
