// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     An expression returned by visitors and translators indicating that the given input could not be translated.
/// </summary>
/// <remarks>
///     When returned from a component (e.g. a method/member translator), this indicates that the input is verified to be untranslatable,
///     and translation should be aborted immediately, without retrying later components.
/// </remarks>
[DebuggerDisplay("{Microsoft.EntityFrameworkCore.Query.ExpressionPrinter.Print(this), nq}")]
public class TranslationFailedExpression : Expression, IPrintableExpression
{
    /// <summary>
    ///     Creates a new instance of the <see cref="TranslationFailedExpression" /> class.
    /// </summary>
    /// <param name="expression">The input expression node that could not be translated.</param>
    /// <param name="message">A message to display to the user, explaining why the translation failed.</param>
    public TranslationFailedExpression(Expression expression, string message)
    {
        Expression = expression;
        Message = message;
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="TranslationFailedExpression" /> class.
    /// </summary>
    /// <param name="expression">The input expression node that could not be translated.</param>
    // TODO: Ideally remove this...
    public TranslationFailedExpression(Expression expression)
        => Expression = expression;

    /// <inheritdoc />
    public override Type Type
        => typeof(void);

    /// <summary>
    ///     The input expression node that could not be translated.
    /// </summary>
    public Expression? Expression { get; }

    /// <summary>
    ///     A message to display to the user, explaining why the translation failed.
    /// </summary>
    public string? Message { get; }

    /// <inheritdoc />
    public override ExpressionType NodeType
        => ExpressionType.Extension;

    void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
        => expressionPrinter.Append(Message is null ? "[UNTRANSLATABLE]" : $"[UNTRANSLATABLE: {Message}]");
}
