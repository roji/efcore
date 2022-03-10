// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
///     Provides an API point for provider-specific extensions for configuring a <see cref="ITrigger" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-triggers">Database triggers</see> for more information and examples.
/// </remarks>
public class TriggerBuilder : IInfrastructure<IConventionTriggerBuilder>
{
    /// <summary>
    ///     Creates a new builder for the given <see cref="ITrigger" />.
    /// </summary>
    /// <param name="trigger">The <see cref="IMutableTrigger" /> to configure.</param>
    public TriggerBuilder(IMutableTrigger trigger)
        => Builder = ((Trigger)trigger).Builder;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    protected virtual InternalTriggerBuilder Builder { [DebuggerStepThrough] get; }

    /// <inheritdoc />
    IConventionTriggerBuilder IInfrastructure<IConventionTriggerBuilder>.Instance
    {
        [DebuggerStepThrough]
        get => Builder;
    }

    /// <summary>
    ///     The trigger being configured.
    /// </summary>
    public virtual IMutableTrigger Metadata
        => Builder.Metadata;

    /// <summary>
    ///     Sets the database name of the trigger.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-triggers">Database triggers</see> for more information and examples.
    /// </remarks>
    /// <param name="name">The database name of the trigger.</param>
    /// <returns>The same builder instance so that multiple configuration calls can be chained.</returns>
    public virtual TriggerBuilder HasName(string name)
    {
        Builder.HasName(name, ConfigurationSource.Explicit);

        return this;
    }
}
