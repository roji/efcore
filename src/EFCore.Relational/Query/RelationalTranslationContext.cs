// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     A context holding state for the current, in-progress translation process.
/// </summary>

// TODO: Maybe consider an interface that's exposed to SqlTranslator, and which only allows the
// operations we need to do there...
public class RelationalTranslationContext
{
    // TODO: Should the outer shaper be Expression? What happens if there's an IncludeExpression wrapper?
    public readonly List<(INavigation Navigation, RelationalStructuralTypeShaperExpression Outer, RelationalStructuralTypeShaperExpression Inner)> PendingJoins = [];
}
