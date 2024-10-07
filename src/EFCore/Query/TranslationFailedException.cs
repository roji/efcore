// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class TranslationFailedException : Exception
{
    private Expression? _node;

    public Expression Node
        => _node ?? throw new InvalidOperationException("Unset node"); // TODO

    public TranslationFailedException(Expression node)
        => _node = node;

    public TranslationFailedException(Expression node, string message)
        : base(message)
        => _node = node;

    // Only used from Method/MemberTranslators which have no access to the original, untranslated
    // node (a new TranslationFailedException is constructed later including the node)
    public TranslationFailedException(string message)
        : base(message)
    {
    }

    // TODO: Go over usages of this, ideally remove.
    public TranslationFailedException()
    {
    }
}
