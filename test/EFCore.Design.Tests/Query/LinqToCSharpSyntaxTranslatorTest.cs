// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Xunit.Sdk;
using static System.Linq.Expressions.Expression;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Query;

public class LinqToCSharpSyntaxTranslatorTest(ITestOutputHelper testOutputHelper)
{
    [Theory]
    [InlineData("hello", "\"hello\"")]
    [InlineData(1, "1")]
    [InlineData(1L, "1L")]
    [InlineData((short)1, "1")]
    [InlineData((sbyte)1, "1")]
    [InlineData(1U, "1U")]
    [InlineData(1UL, "1UL")]
    [InlineData((ushort)1, "1")]
    [InlineData((byte)1, "1")]
    [InlineData(1.5, "1.5D")]
    [InlineData(1.5F, "1.5F")]
    [InlineData(true, "true")]
    [InlineData(typeof(string), "typeof(string)")]
    public void Constant_values(object constantValue, string literalRepresentation)
        => AssertExpression(literalRepresentation, Constant(constantValue));

    [Fact]
    public void Constant_DateTime_default()
        => AssertExpression("default(DateTime)", Constant(default(DateTime)));

    [Fact]
    public void Constant_decimal()
        => AssertExpression("1.5M", Constant(1.5m));

    [Fact]
    public void Constant_null()
        => AssertExpression("null", Constant(null, typeof(string)));

    [Fact]
    public void Constant_throws_on_unsupported_type()
        => Assert.Throws<NotSupportedException>(() => AssertExpression("", Constant(DateTime.Now)));

    [Fact]
    public void Enum()
        => AssertExpression("LinqToCSharpSyntaxTranslatorTest.SomeEnum.One", Constant(SomeEnum.One));

    [Fact]
    public void Enum_with_multiple_values()
        => AssertExpression("LinqToCSharpSyntaxTranslatorTest.SomeEnum.One | LinqToCSharpSyntaxTranslatorTest.SomeEnum.Two", Constant(SomeEnum.One | SomeEnum.Two));

    [Fact]
    public void Enum_with_unknown_value()
        => AssertExpression("(LinqToCSharpSyntaxTranslatorTest.SomeEnum)1000L", Constant((SomeEnum)1000));

    [Theory]
    [InlineData(ExpressionType.Add, "+")]
    [InlineData(ExpressionType.Subtract, "-")]
    [InlineData(ExpressionType.Assign, "=")]
    [InlineData(ExpressionType.AddAssign, "+=")]
    [InlineData(ExpressionType.AddAssignChecked, "+=")]
    [InlineData(ExpressionType.MultiplyAssign, "*=")]
    [InlineData(ExpressionType.MultiplyAssignChecked, "*=")]
    [InlineData(ExpressionType.DivideAssign, "/=")]
    [InlineData(ExpressionType.ModuloAssign, "%=")]
    [InlineData(ExpressionType.SubtractAssign, "-=")]
    [InlineData(ExpressionType.SubtractAssignChecked, "-=")]
    [InlineData(ExpressionType.AndAssign, "&=")]
    [InlineData(ExpressionType.OrAssign, "|=")]
    [InlineData(ExpressionType.LeftShiftAssign, "<<=")]
    [InlineData(ExpressionType.RightShiftAssign, ">>=")]
    [InlineData(ExpressionType.ExclusiveOrAssign, "^=")]
    public void Binary_numeric(ExpressionType expressionType, string op)
        => AssertExpression($"i {op} 3", MakeBinary(expressionType, Parameter(typeof(int), "i"), Constant(3)));

    [Fact]
    public void Binary_ArrayIndex()
        => AssertExpression("i[2]", ArrayIndex(Parameter(typeof(int[]), "i"), Constant(2)));

    [Fact]
    public void Binary_Power()
        => AssertExpression("Math.Pow(2D, 3D)", Power(Constant(2.0), Constant(3.0)));

    [Fact]
    public void Binary_PowerAssign()
        => AssertExpression("d = Math.Pow(d, 3D)", PowerAssign(Parameter(typeof(double), "d"), Constant(3.0)));

    [Fact]
    public void Private_instance_field_SimpleAssign()
        => AssertExpression("""typeof(LinqToCSharpSyntaxTranslatorTest.Blog).GetField("_privateField", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).SetValue(blog, 3)""", Assign(
            Field(Parameter(typeof(Blog), "blog"), "_privateField"),
            Constant(3)));

    [Theory]
    [InlineData(ExpressionType.AddAssign, "+")]
    [InlineData(ExpressionType.MultiplyAssign, "*")]
    [InlineData(ExpressionType.DivideAssign, "/")]
    [InlineData(ExpressionType.ModuloAssign, "%")]
    [InlineData(ExpressionType.SubtractAssign, "-")]
    [InlineData(ExpressionType.AndAssign, "&")]
    [InlineData(ExpressionType.OrAssign, "|")]
    [InlineData(ExpressionType.LeftShiftAssign, "<<")]
    [InlineData(ExpressionType.RightShiftAssign, ">>")]
    [InlineData(ExpressionType.ExclusiveOrAssign, "^")]
    public void Private_instance_field_AssignOperators(ExpressionType expressionType, string op)
        => AssertExpression($"""typeof(LinqToCSharpSyntaxTranslatorTest.Blog).GetField("_privateField", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).SetValue(blog, (int)typeof(LinqToCSharpSyntaxTranslatorTest.Blog).GetField("_privateField", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).GetValue(blog) {op} 3)""", MakeBinary(
            expressionType,
            Field(Parameter(typeof(Blog), "blog"), "_privateField"),
            Constant(3)));

    [Theory]
    [InlineData(ExpressionType.AddAssign, "+")]
    [InlineData(ExpressionType.MultiplyAssign, "*")]
    [InlineData(ExpressionType.DivideAssign, "/")]
    [InlineData(ExpressionType.ModuloAssign, "%")]
    [InlineData(ExpressionType.SubtractAssign, "-")]
    [InlineData(ExpressionType.AndAssign, "&")]
    [InlineData(ExpressionType.OrAssign, "|")]
    [InlineData(ExpressionType.LeftShiftAssign, "<<")]
    [InlineData(ExpressionType.RightShiftAssign, ">>")]
    [InlineData(ExpressionType.ExclusiveOrAssign, "^")]
    public void Private_instance_field_AssignOperators_with_replacements(ExpressionType expressionType, string op)
        => AssertExpression(
            $"""WritePrivateField(blog, ReadPrivateField(blog) {op} Three)""",
            MakeBinary(
                expressionType,
                Field(Parameter(typeof(Blog), "blog"), "_privateField"),
                Constant(3)),
            new Dictionary<object, string>() { { 3, "Three" } }, new Dictionary<MemberAccess, string>() {
                { new MemberAccess(BlogPrivateField, assignment: true), "WritePrivateField" },
                { new MemberAccess(BlogPrivateField, assignment: false), "ReadPrivateField" }
            });

    [Theory]
    [InlineData(ExpressionType.Negate, "-i")]
    [InlineData(ExpressionType.NegateChecked, "-i")]
    [InlineData(ExpressionType.Not, "~i")]
    [InlineData(ExpressionType.OnesComplement, "~i")]
    [InlineData(ExpressionType.UnaryPlus, "+i")]
    [InlineData(ExpressionType.Increment, "i + 1")]
    [InlineData(ExpressionType.Decrement, "i - 1")]
    public void Unary_expression_int(ExpressionType expressionType, string expected)
        => AssertExpression(expected, MakeUnary(expressionType, Parameter(typeof(int), "i"), typeof(int)));

    [Theory]
    [InlineData(ExpressionType.Not, "!b")]
    [InlineData(ExpressionType.IsFalse, "!b")]
    [InlineData(ExpressionType.IsTrue, "b")]
    public void Unary_expression_bool(ExpressionType expressionType, string expected)
        => AssertExpression(expected, MakeUnary(expressionType, Parameter(typeof(bool), "b"), typeof(bool)));

    [Theory]
    [InlineData(ExpressionType.PostIncrementAssign, "i++")]
    [InlineData(ExpressionType.PostDecrementAssign, "i--")]
    [InlineData(ExpressionType.PreIncrementAssign, "++i")]
    [InlineData(ExpressionType.PreDecrementAssign, "--i")]
    public void Unary_statement(ExpressionType expressionType, string expected)
    {
        var i = Parameter(typeof(int), "i");

        AssertStatement(
            $$"""
{
    int i;
    {{expected}};
}
""", Block(
                variables: [i],
                MakeUnary(expressionType, i, typeof(int))));
    }

    [Fact]
    public void Unary_ArrayLength()
        => AssertExpression("i.Length", ArrayLength(Parameter(typeof(int[]), "i")));

    [Fact]
    public void Unary_Convert()
        => AssertExpression("(string)i", Convert(
            Parameter(typeof(object), "i"),
            typeof(string)));

    [Fact]
    public void Unary_Throw()
        => AssertStatement("throw new Exception();", Throw(New(typeof(Exception))));

    [Fact]
    public void Unary_Unbox()
        => AssertExpression("i", Unbox(Parameter(typeof(object), "i"), typeof(int)));

    [Fact]
    public void Unary_Quote()
        => AssertExpression("(string s) => s.Length", Quote((Expression<Func<string, int>>)(s => s.Length)));

    [Fact]
    public void Unary_TypeAs()
        => AssertExpression("i as string", TypeAs(Parameter(typeof(object), "i"), typeof(string)));

    [Fact]
    public void Instance_property()
        => AssertExpression("\"hello\".Length", Property(
            Constant("hello"),
            typeof(string).GetProperty(nameof(string.Length))!));

    [Fact]
    public void Static_property()
        => AssertExpression("DateTime.Now", Property(
            null,
            typeof(DateTime).GetProperty(nameof(DateTime.Now))!));

    [Fact]
    public void Indexer_property()
        => AssertExpression("new List<int>()[1]", Call(
            New(typeof(List<int>)),
            typeof(List<int>).GetProperties().Single(
                    p => p.GetIndexParameters() is { Length: 1 } indexParameters && indexParameters[0].ParameterType == typeof(int))
                .GetMethod!,
            Constant(1)));

    [Fact]
    public void Private_instance_field_read()
        => AssertExpression("""(int)typeof(LinqToCSharpSyntaxTranslatorTest.Blog).GetField("_privateField", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).GetValue(blog)""", Field(Parameter(typeof(Blog), "blog"), "_privateField"));

    [Fact]
    public void Private_instance_field_write()
        => AssertStatement("""typeof(LinqToCSharpSyntaxTranslatorTest.Blog).GetField("_privateField", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).SetValue(blog, 8)""", Assign(
            Field(Parameter(typeof(Blog), "blog"), "_privateField"),
            Constant(8)));

    [Fact]
    public void Internal_instance_field_read()
        => AssertExpression("""(int)typeof(LinqToCSharpSyntaxTranslatorTest.Blog).GetField("InternalField", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).GetValue(blog)""", Field(Parameter(typeof(Blog), "blog"), "InternalField"));

    [Fact]
    public void Not()
        => AssertExpression("!true", Expression.Not(Constant(true)));

    [Fact]
    public void MemberInit_with_MemberAssignment()
        => AssertExpression(
            """
new LinqToCSharpSyntaxTranslatorTest.Blog("foo")
{
    PublicProperty = 8,
    PublicField = 9
}
""", MemberInit(
                New(
                    typeof(Blog).GetConstructor([typeof(string)])!,
                    Constant("foo")),
                Bind(typeof(Blog).GetProperty(nameof(Blog.PublicProperty))!, Constant(8)),
                Bind(typeof(Blog).GetField(nameof(Blog.PublicField))!, Constant(9))));

    [Fact]
    public void MemberInit_with_MemberListBinding()
        => AssertExpression(
            """
new LinqToCSharpSyntaxTranslatorTest.Blog("foo")
{
    ListOfInts =
    {
        8,
        9
    }
}
""", MemberInit(
                New(
                    typeof(Blog).GetConstructor([typeof(string)])!,
                    Constant("foo")),
                ListBind(
                    typeof(Blog).GetProperty(nameof(Blog.ListOfInts))!,
                    ElementInit(typeof(List<int>).GetMethod(nameof(List<int>.Add))!, Constant(8)),
                    ElementInit(typeof(List<int>).GetMethod(nameof(List<int>.Add))!, Constant(9)))));

    [Fact]
    public void MemberInit_with_MemberMemberBinding()
        => AssertExpression(
            """
new LinqToCSharpSyntaxTranslatorTest.Blog("foo")
{
    Details =
    {
        Foo = 5,
        ListOfInts =
        {
            8,
            9
        }
    }
}
""", MemberInit(
                New(
                    typeof(Blog).GetConstructor([typeof(string)])!,
                    Constant("foo")),
                MemberBind(
                    typeof(Blog).GetProperty(nameof(Blog.Details))!,
                    Bind(typeof(BlogDetails).GetProperty(nameof(BlogDetails.Foo))!, Constant(5)),
                    ListBind(
                        typeof(BlogDetails).GetProperty(nameof(BlogDetails.ListOfInts))!,
                        ElementInit(typeof(List<int>).GetMethod(nameof(List<int>.Add))!, Constant(8)),
                        ElementInit(typeof(List<int>).GetMethod(nameof(List<int>.Add))!, Constant(9))))));

    [Fact]
    public void Method_call_instance()
    {
        var blog = Parameter(typeof(Blog), "blog");

        AssertStatement(
            """
{
    var blog = new LinqToCSharpSyntaxTranslatorTest.Blog();
    blog.SomeInstanceMethod();
}
""", Block(
                variables: [blog],
                Assign(blog, New(Blog.Constructor)),
                Call(
                    blog,
                    typeof(Blog).GetMethod(nameof(Blog.SomeInstanceMethod))!)));
    }

    [Fact]
    public void Method_call_static()
        => AssertExpression("LinqToCSharpSyntaxTranslatorTest.ReturnsIntWithParam(8)", Call(ReturnsIntWithParamMethod, Constant(8)));

    [Fact]
    public void Method_call_static_on_nested_type()
        => AssertExpression("LinqToCSharpSyntaxTranslatorTest.Blog.Static_method_on_nested_type()", Call(typeof(Blog).GetMethod(nameof(Blog.Static_method_on_nested_type))!));

    [Fact]
    public void Method_call_extension()
    {
        var blog = Parameter(typeof(LinqExpressionToRoslynTranslatorExtensionType), "someType");

        AssertStatement(
            """
{
    var someType = new LinqExpressionToRoslynTranslatorExtensionType();
    someType.SomeExtension();
}
""", Block(
                variables: [blog],
                Assign(blog, New(LinqExpressionToRoslynTranslatorExtensionType.Constructor)),
                Call(LinqExpressionToRoslynTranslatorExtensions.SomeExtensionMethod, blog)));
    }

    [Fact]
    public void Method_call_extension_with_null_this()
        => AssertExpression("LinqExpressionToRoslynTranslatorExtensions.SomeExtension(null)", Call(
            LinqExpressionToRoslynTranslatorExtensions.SomeExtensionMethod,
            Constant(null, typeof(LinqExpressionToRoslynTranslatorExtensionType))));

    [Fact]
    public void Method_call_generic()
    {
        var blog = Parameter(typeof(Blog), "blog");

        AssertStatement(
            """
{
    var blog = new LinqToCSharpSyntaxTranslatorTest.Blog();
    LinqToCSharpSyntaxTranslatorTest.GenericMethodImplementation(blog);
}
""", Block(
                variables: [blog],
                Assign(blog, New(Blog.Constructor)),
                Call(
                    GenericMethod.MakeGenericMethod(typeof(Blog)),
                    blog)));
    }

    [Fact]
    public void Method_call_namespace_is_collected()
    {
        var (translator, _) = CreateTranslator();
        var namespaces = new HashSet<string>();
        _ = translator.TranslateExpression(Call(FooMethod), null, namespaces);
        Assert.Collection(
            namespaces,
            ns => Assert.Equal(typeof(LinqToCSharpSyntaxTranslatorTest).Namespace, ns));
    }

    [Fact]
    public void Method_call_with_in_out_ref_parameters()
    {
        var inParam = Parameter(typeof(int), "inParam");
        var outParam = Parameter(typeof(int), "outParam");
        var refParam = Parameter(typeof(int), "refParam");

        AssertStatement(
            """
{
    int inParam;
    int outParam;
    int refParam;
    LinqToCSharpSyntaxTranslatorTest.WithInOutRefParameter(in inParam, out outParam, ref refParam);
}
""", Block(
                variables: [inParam, outParam, refParam],
                Call(WithInOutRefParameterMethod, [inParam, outParam, refParam])));
    }

    [Fact]
    public void Instantiation()
        => AssertExpression("""new LinqToCSharpSyntaxTranslatorTest.Blog("foo")""", New(
            typeof(Blog).GetConstructor([typeof(string)])!,
            Constant("foo")));

    [Fact]
    public void Instantiation_with_required_properties_and_parameterless_constructor()
        => AssertExpression(
            """
Activator.CreateInstance<LinqToCSharpSyntaxTranslatorTest.BlogWithRequiredProperties>()
""", New(
                typeof(BlogWithRequiredProperties).GetConstructor([])!));

    [Fact]
    public void Instantiation_with_required_properties_and_non_parameterless_constructor()
        => Assert.Throws<NotImplementedException>(
            () => AssertExpression("", New(
                typeof(BlogWithRequiredProperties).GetConstructor([typeof(string)])!,
                Constant("foo"))));

    [Fact]
    public void Instantiation_with_required_properties_with_SetsRequiredMembers()
        => AssertExpression("""new LinqToCSharpSyntaxTranslatorTest.BlogWithRequiredProperties("foo", 8)""", New(
            typeof(BlogWithRequiredProperties).GetConstructor([typeof(string), typeof(int)])!,
            Constant("foo"), Constant(8)));

    [Fact]
    public void Lambda_with_expression_body()
        => AssertExpression("() => true", Lambda<Func<bool>>(Constant(true)));

    [Fact]
    public void Lambda_with_block_body()
    {
        var i = Parameter(typeof(int), "i");

        AssertExpression(
            """
() =>
{
    var i = 8;
    return i;
}
""", Lambda<Func<int>>(
                Block(
                    variables: [i],
                    Assign(i, Constant(8)),
                    i)));
    }

    [Fact]
    public void Lambda_with_no_parameters()
        => AssertExpression("() => true", Lambda<Func<bool>>(Constant(true)));

    [Fact]
    public void Lambda_with_one_parameter()
    {
        var i = Parameter(typeof(int), "i");

        AssertExpression("(int i) => true", Lambda<Func<int, bool>>(Constant(true), i));
    }

    [Fact]
    public void Lambda_with_two_parameters()
    {
        var i = Parameter(typeof(int), "i");
        var j = Parameter(typeof(int), "j");

        AssertExpression("(int i, int j) => i + j", Lambda<Func<int, int, int>>(Add(i, j), i, j));
    }

    [Fact]
    public void Invocation_with_literal_argument()
        => AssertExpression("true && 8 > 5", AndAlso(
            Constant(true),
            Invoke((Expression<Func<int, bool>>)(f => f > 5), Constant(8))));

    [Fact]
    public void Invocation_with_argument_that_has_side_effects()
    {
        var i = Parameter(typeof(int), "i");

        AssertStatement(
            """
{
    var f = LinqToCSharpSyntaxTranslatorTest.Foo();
    var i = 5 + f + f;
}
""", Block(
                variables: [i],
                Assign(
                    i,
                    Add(
                        Constant(5),
                        Invoke((Expression<Func<int, int>>)(f => f + f), Call(FooMethod))))));
    }

    [Fact]
    public void Conditional_expression()
        => AssertExpression("true ? 1 : 2", Condition(Constant(true), Constant(1), Constant(2)));

    [Fact]
    public void Conditional_without_false_value_fails()
        => Assert.Throws<NotSupportedException>(
            () => AssertExpression("true ? 1 : 2", IfThen(Constant(true), Constant(8))));

    [Fact]
    public void Conditional_statement()
        => AssertStatement(
            """
{
    if (true)
    {
        LinqToCSharpSyntaxTranslatorTest.Foo();
    }
    else
    {
        LinqToCSharpSyntaxTranslatorTest.Bar();
    }
}
""", Block(
                Condition(Constant(true), Call(FooMethod), Call(BarMethod)),
                Constant(8)));

    [Fact]
    public void IfThen_statement()
    {
        var parameter = Parameter(typeof(int), "i");
        var block = Block(
            variables: [parameter],
            expressions: Assign(parameter, Constant(8)));

        AssertStatement(
            """
{
    if (true)
    {
        var i = 8;
    }
}
""", Block(IfThen(Constant(true), block)));
    }

    [Fact]
    public void IfThenElse_statement()
    {
        var parameter1 = Parameter(typeof(int), "i");
        var block1 = Block(
            variables: [parameter1],
            expressions: Assign(parameter1, Constant(8)));

        var parameter2 = Parameter(typeof(int), "j");
        var block2 = Block(
            variables: [parameter2],
            expressions: Assign(parameter2, Constant(9)));

        AssertStatement(
            """
{
    if (true)
    {
        var i = 8;
    }
    else
    {
        var j = 9;
    }
}
""", Block(IfThenElse(Constant(true), block1, block2)));
    }

    [Fact]
    public void IfThenElse_nested()
    {
        var variable = Parameter(typeof(int), "i");

        AssertStatement(
            """
{
    int i;
    if (true)
    {
        i = 1;
    }
    else if (false)
    {
        i = 2;
    }
    else
    {
        i = 3;
    }
}
""", Block(
                variables: [variable],
                expressions: IfThenElse(
                    Constant(true),
                    Block(Assign(variable, Constant(1))),
                    IfThenElse(
                        Constant(false),
                        Block(Assign(variable, Constant(2))),
                        Block(Assign(variable, Constant(3)))))));
    }

    [Fact]
    public void Conditional_expression_with_block_in_lambda()
        => AssertExpression(
            """
() =>
{
    if (true)
    {
        LinqToCSharpSyntaxTranslatorTest.Foo();
        return 8;
    }
    else
    {
        return 9;
    }
}
""", Lambda<Func<int>>(
                Condition(
                    Constant(true),
                    Block(
                        Call(FooMethod),
                        Constant(8)),
                    Constant(9))));

    [Fact]
    public void IfThen_with_block_inside_expression_block_with_lifted_statements()
    {
        var i = Parameter(typeof(int), "i");

        AssertStatement(
            """
{
    LinqToCSharpSyntaxTranslatorTest.Foo();
    if (true)
    {
        LinqToCSharpSyntaxTranslatorTest.Bar();
        LinqToCSharpSyntaxTranslatorTest.Baz();
    }

    var i = 8;
}
""", Block(
                variables: [i],
                Assign(
                    i, Block(
                        // We're in expression context. Do anything that will get lifted.
                        Call(FooMethod),
                        // Statement condition
                        IfThen(
                            Constant(true),
                            Block(
                                Call(BarMethod),
                                Call(BazMethod))),
                        // Last expression (to make the block above evaluate as statement
                        Constant(8)))));
    }

    [Fact]
    public void Switch_expression()
        => AssertExpression(
            """
8 switch
{
    9 => -9,
    10 => -10,
    _ => 0
}
""", Switch(
                Constant(8),
                Constant(0),
                SwitchCase(Constant(-9), Constant(9)),
                SwitchCase(Constant(-10), Constant(10))));

    [Fact]
    public void Switch_expression_nested()
    {
        var i = Parameter(typeof(int), "i");
        var j = Parameter(typeof(int), "j");
        var k = Parameter(typeof(int), "k");

        AssertStatement(
            """
{
    int k;
    var j = 8;
    var i = j switch
    {
        100 => 1,
        200 => k switch
        {
            200 => 2,
            300 => 3,
            _ => 0
        },
        _ => 0
    };
}
""", Block(
                variables: [i, j, k],
                Assign(j, Constant(8)),
                Assign(
                    i,
                    Switch(
                        j,
                        defaultBody: Constant(0),
                        SwitchCase(Constant(1), Constant(100)),
                        SwitchCase(
                            Switch(
                                k,
                                defaultBody: Constant(0),
                                SwitchCase(Constant(2), Constant(200)),
                                SwitchCase(Constant(3), Constant(300))),
                            Constant(200))))));
    }

    [Fact]
    public void Switch_expression_non_constant_arm()
        => AssertExpression("blog1 == blog2 ? 2 : blog1 == blog3 ? 3 : 0", Switch(
            Parameter(typeof(Blog), "blog1"),
            Constant(0),
            SwitchCase(Constant(2), Parameter(typeof(Blog), "blog2")),
            SwitchCase(Constant(3), Parameter(typeof(Blog), "blog3"))));

    [Fact]
    public void Switch_statement_with_non_constant_label()
        => AssertStatement(
            """
if (blog1 == blog2)
{
    2;
}
else if (blog1 == blog3)
{
    3;
}
else
{
    0;
}
""", Switch(
                Parameter(typeof(Blog), "blog1"),
                Constant(0),
                SwitchCase(Constant(2), Parameter(typeof(Blog), "blog2")),
                SwitchCase(Constant(3), Parameter(typeof(Blog), "blog3"))));

    [Fact]
    public void Switch_statement_without_default()
    {
        var parameter = Parameter(typeof(int), "i");

        AssertStatement(
            """
{
    int i;
    switch (7)
    {
        case -9:
        {
            i = 9;
            break;
        }

        case -10:
        {
            i = 10;
            break;
        }
    }
}
""", Block(
                variables: [parameter],
                expressions: Switch(
                    Constant(7),
                    SwitchCase(Block(typeof(void), Assign(parameter, Constant(9))), Constant(-9)),
                    SwitchCase(Block(typeof(void), Assign(parameter, Constant(10))), Constant(-10)))));
    }

    [Fact]
    public void Switch_statement_with_default()
    {
        var parameter = Parameter(typeof(int), "i");

        AssertStatement(
            """
{
    int i;
    switch (7)
    {
        case -9:
            i = 9;
            break;
        case -10:
            i = 10;
            break;
        default:
            i = 0;
            break;
    }
}
""", Block(
                variables: [parameter],
                expressions: Switch(
                    Constant(7),
                    Assign(parameter, Constant(0)),
                    SwitchCase(Assign(parameter, Constant(9)), Constant(-9)),
                    SwitchCase(Assign(parameter, Constant(10)), Constant(-10)))));
    }

    [Fact]
    public void Switch_statement_with_multiple_labels()
    {
        var parameter = Parameter(typeof(int), "i");

        AssertStatement(
            """
{
    int i;
    switch (7)
    {
        case -9:
        case -8:
            i = 9;
            break;
        case -10:
            i = 10;
            break;
        default:
            i = 0;
            break;
    }
}
""", Block(
                variables: [parameter],
                expressions: Switch(
                    Constant(7),
                    Assign(parameter, Constant(0)),
                    SwitchCase(Assign(parameter, Constant(9)), Constant(-9), Constant(-8)),
                    SwitchCase(Assign(parameter, Constant(10)), Constant(-10)))));
    }

    [Fact]
    public void Variable_assignment_uses_var()
    {
        var i = Parameter(typeof(int), "i");

        AssertStatement(
            """
{
    var i = 8;
}
""", Block(
                variables: [i],
                Assign(i, Constant(8))));
    }

    [Fact]
    public void Variable_assignment_to_null_does_not_use_var()
    {
        var s = Parameter(typeof(string), "s");

        AssertStatement(
            """
{
    string s = null;
}
""", Block(
                variables: [s],
                Assign(s, Constant(null, typeof(string)))));
    }

    [Fact]
    public void Variables_with_same_name_in_sibling_blocks_do_not_get_renamed()
    {
        var i1 = Parameter(typeof(int), "i");
        var i2 = Parameter(typeof(int), "i");

        AssertStatement(
            """
{
    {
        var i = 8;
        LinqToCSharpSyntaxTranslatorTest.ReturnsIntWithParam(i);
    }

    {
        var i = 8;
        LinqToCSharpSyntaxTranslatorTest.ReturnsIntWithParam(i);
    }
}
""", Block(
                Block(
                    variables: [i1],
                    Assign(i1, Constant(8)),
                    Call(ReturnsIntWithParamMethod, i1)),
                Block(
                    variables: [i2],
                    Assign(i2, Constant(8)),
                    Call(ReturnsIntWithParamMethod, i2))));
    }

    [Fact]
    public void Variable_with_same_name_in_child_block_gets_renamed()
    {
        var i1 = Parameter(typeof(int), "i");
        var i2 = Parameter(typeof(int), "i");

        AssertStatement(
            """
{
    var i = 8;
    LinqToCSharpSyntaxTranslatorTest.ReturnsIntWithParam(i);
    {
        var i0 = 8;
        LinqToCSharpSyntaxTranslatorTest.ReturnsIntWithParam(i0);
        LinqToCSharpSyntaxTranslatorTest.ReturnsIntWithParam(i);
    }
}
""", Block(
                variables: [i1],
                Assign(i1, Constant(8)),
                Call(ReturnsIntWithParamMethod, i1),
                Block(
                    variables: [i2],
                    Assign(i2, Constant(8)),
                    Call(ReturnsIntWithParamMethod, i2),
                    Call(ReturnsIntWithParamMethod, i1))));
    }

    [Fact]
    public void Variable_with_same_name_in_lambda_does_not_get_renamed()
    {
        var i1 = Parameter(typeof(int), "i");
        var i2 = Parameter(typeof(int), "i");
        var f = Parameter(typeof(Func<int, bool>), "f");

        AssertStatement(
            """
{
    var i = 8;
    f = (int i) => i == 5;
}
""", Block(
                variables: [i1],
                Assign(i1, Constant(8)),
                Assign(
                    f, Lambda<Func<int, bool>>(
                        Equal(i2, Constant(5)),
                        i2))));
    }

    [Fact]
    public void Same_parameter_instance_is_used_twice_in_nested_lambdas()
    {
        var f1 = Parameter(typeof(Func<int, bool>), "f1");
        var f2 = Parameter(typeof(Func<int, bool>), "f2");
        var i = Parameter(typeof(int), "i");

        AssertExpression(
            """
f1 = (int i) =>
{
    f2 = (int i) => i == 5;
    return true;
}
""", Assign(
                f1,
                Lambda<Func<int, bool>>(
                    Block(
                        Assign(
                            f2,
                            Lambda<Func<int, bool>>(
                                Equal(i, Constant(5)),
                                i)),
                        Constant(true)),
                    i)));
    }

    [Fact]
    public void Block_with_non_standalone_expression_as_statement()
        => AssertStatement(
            """
{
    _ = 1 + 2;
}
""", Block(Add(Constant(1), Constant(2))));

    [Fact]
    public void Lift_block_in_assignment_context()
    {
        var i = Parameter(typeof(int), "i");
        var j = Parameter(typeof(int), "j");

        AssertStatement(
            """
{
    var j = LinqToCSharpSyntaxTranslatorTest.Foo();
    var i = LinqToCSharpSyntaxTranslatorTest.ReturnsIntWithParam(j);
}
""", Block(
                variables: [i],
                Assign(
                    i, Block(
                        variables: [j],
                        Assign(j, Call(FooMethod)),
                        Call(ReturnsIntWithParamMethod, j)))));
    }

    [Fact]
    public void Lift_block_in_method_call_context()
        => AssertStatement(
            """
{
    LinqToCSharpSyntaxTranslatorTest.Foo();
    LinqToCSharpSyntaxTranslatorTest.ReturnsIntWithParam(LinqToCSharpSyntaxTranslatorTest.Bar());
}
""", Block(
                Call(
                    ReturnsIntWithParamMethod,
                    Block(
                        Call(FooMethod),
                        Call(BarMethod)))));

    [Fact]
    public void Lift_nested_block()
    {
        var i = Parameter(typeof(int), "i");
        var j = Parameter(typeof(int), "j");

        AssertStatement(
            """
{
    var j = LinqToCSharpSyntaxTranslatorTest.Foo();
    LinqToCSharpSyntaxTranslatorTest.Bar();
    var i = LinqToCSharpSyntaxTranslatorTest.ReturnsIntWithParam(j);
}
""", Block(
                variables: [i],
                Assign(
                    i,
                    Block(
                        variables: [j],
                        Assign(j, Call(FooMethod)),
                        Block(
                            Call(BarMethod),
                            Call(ReturnsIntWithParamMethod, j))))));
    }

    [Fact]
    public void Binary_lifts_left_side_if_right_is_lifted()
    {
        var i = Parameter(typeof(int), "i");

        AssertStatement(
            """
{
    var lifted = LinqToCSharpSyntaxTranslatorTest.Foo();
    LinqToCSharpSyntaxTranslatorTest.Bar();
    var i = lifted + LinqToCSharpSyntaxTranslatorTest.Baz();
}
""", Block(
                variables: [i],
                Assign(
                    i,
                    Add(
                        Call(FooMethod),
                        Block(
                            Call(BarMethod),
                            Call(BazMethod))))));
    }

    [Fact]
    public void Binary_does_not_lift_left_side_if_it_has_no_side_effects()
    {
        var i = Parameter(typeof(int), "i");

        AssertStatement(
            """
{
    LinqToCSharpSyntaxTranslatorTest.Bar();
    var i = 5 + LinqToCSharpSyntaxTranslatorTest.Baz();
}
""", Block(
                variables: [i],
                Assign(
                    i,
                    Add(
                        Constant(5),
                        Block(
                            Call(BarMethod),
                            Call(BazMethod))))));
    }

    [Fact]
    public void Method_lifts_earlier_args_if_later_arg_is_lifted()
    {
        var i = Parameter(typeof(int), "i");

        AssertStatement(
            """
{
    var liftedArg = LinqToCSharpSyntaxTranslatorTest.Foo();
    LinqToCSharpSyntaxTranslatorTest.Bar();
    var liftedArg0 = LinqToCSharpSyntaxTranslatorTest.Baz();
    var liftedArg1 = LinqToCSharpSyntaxTranslatorTest.Foo();
    LinqToCSharpSyntaxTranslatorTest.Baz();
    var i = LinqToCSharpSyntaxTranslatorTest.MethodWithSixParams(liftedArg, 5, liftedArg0, liftedArg1, LinqToCSharpSyntaxTranslatorTest.Bar(), LinqToCSharpSyntaxTranslatorTest.Foo());
}
""", Block(
                variables: [i],
                Assign(
                    i,
                    Call(
                        typeof(LinqToCSharpSyntaxTranslatorTest).GetMethod(nameof(MethodWithSixParams))!,
                        Call(FooMethod),
                        Constant(5),
                        Block(Call(BarMethod), Call(BazMethod)),
                        Call(FooMethod),
                        Block(Call(BazMethod), Call(BarMethod)),
                        Call(FooMethod)))));
    }

    [Fact]
    public void New_lifts_earlier_args_if_later_arg_is_lifted()
    {
        var b = Parameter(typeof(Blog), "b");

        AssertStatement(
            """
{
    var liftedArg = LinqToCSharpSyntaxTranslatorTest.Foo();
    LinqToCSharpSyntaxTranslatorTest.Bar();
    var b = new LinqToCSharpSyntaxTranslatorTest.Blog(liftedArg, LinqToCSharpSyntaxTranslatorTest.Baz());
}
""", Block(
                variables: [b],
                Assign(
                    b,
                    New(
                        typeof(Blog).GetConstructor([typeof(int), typeof(int)])!,
                        Call(FooMethod),
                        Block(
                            Call(BarMethod),
                            Call(BazMethod))))));
    }

    [Fact]
    public void Index_lifts_earlier_args_if_later_arg_is_lifted()
    {
        // TODO: Implement
    }

    [Fact]
    public void New_array()
        => AssertExpression(
            """
new int[]
{
}
""", NewArrayInit(typeof(int)));

    [Fact]
    public void New_array_with_bounds()
        => AssertExpression("new int[3]", NewArrayBounds(typeof(int), Constant(3)));

    [Fact]
    public void New_array_with_initializers()
        => AssertExpression(
            """
new int[]
{
    3,
    4
}
""", NewArrayInit(typeof(int), Constant(3), Constant(4)));

    [Fact]
    public void New_array_lifts_earlier_args_if_later_arg_is_lifted()
    {
        var a = Parameter(typeof(int[]), "a");

        // a = new[] { Foo(), { Bar(); Baz(); } }
        AssertStatement(
            """
{
    var liftedArg = LinqToCSharpSyntaxTranslatorTest.Foo();
    LinqToCSharpSyntaxTranslatorTest.Bar();
    var a = new int[]
    {
        liftedArg,
        LinqToCSharpSyntaxTranslatorTest.Baz()
    };
}
""", Block(
                variables: [a],
                Assign(
                    a,
                    NewArrayInit(
                        typeof(int),
                        Call(FooMethod),
                        Block(
                            Call(BarMethod),
                            Call(BazMethod))))));
    }

    [Fact]
    public void Lift_variable_in_expression_block()
    {
        var i = Parameter(typeof(int), "i");
        var j = Parameter(typeof(int), "j");

        AssertStatement(
            """
{
    int j;
    LinqToCSharpSyntaxTranslatorTest.Foo();
    j = 8;
    var i = 9;
}
""", Block(
                variables: [i],
                Assign(
                    i, Block(
                        variables: [j],
                        Block(
                            Call(FooMethod),
                            Assign(j, Constant(8)),
                            Constant(9))))));
    }

    [Fact]
    public void Lift_block_in_lambda_body_expression()
        => AssertExpression(
            """
() =>
{
    LinqToCSharpSyntaxTranslatorTest.Foo();
    return LinqToCSharpSyntaxTranslatorTest.ReturnsIntWithParam(LinqToCSharpSyntaxTranslatorTest.Bar());
}
""", Lambda<Func<int>>(
                Call(
                    ReturnsIntWithParamMethod,
                    Block(
                        Call(FooMethod),
                        Call(BarMethod))),
                []));

    [Fact]
    public void Do_not_lift_block_in_lambda_body()
        => AssertExpression(
            """
() =>
{
    {
        return 8;
    }
}
""", Lambda<Func<int>>(
                Block(Block(Constant(8))),
                []));

    [Fact]
    public void Simplify_block_with_single_expression()
        => AssertExpression("i = 8", Assign(Parameter(typeof(int), "i"), Block(Constant(8))));

    [Fact]
    public void Cannot_lift_out_of_expression_context()
        => Assert.Throws<NotSupportedException>(
            () => AssertExpression("", Assign(
                Parameter(typeof(int), "i"),
                Block(
                    Call(FooMethod),
                    Constant(8)))));

    [Fact]
    public void Lift_switch_expression()
    {
        var i = Parameter(typeof(int), "i");
        var j = Parameter(typeof(int), "j");
        var k = Parameter(typeof(int), "k");

        AssertStatement(
            """
{
    int i;
    var j = 8;
    switch (j)
    {
        case 8:
        {
            k = LinqToCSharpSyntaxTranslatorTest.Foo();
            i = LinqToCSharpSyntaxTranslatorTest.ReturnsIntWithParam(k);
            break;
        }

        case 9:
            i = 2;
            break;
        default:
            i = 0;
            break;
    }
}
""", Block(
                variables: [i, j],
                Assign(j, Constant(8)),
                Assign(
                    i,
                    Switch(
                        j,
                        defaultBody: Block(Constant(0)),
                        SwitchCase(
                            Block(
                                Block(
                                    Assign(k, Call(FooMethod)),
                                    Call(ReturnsIntWithParamMethod, k))),
                            Constant(8)),
                        SwitchCase(Constant(2), Constant(9))))));
    }

    [Fact]
    public void Lift_nested_switch_expression()
    {
        var i = Parameter(typeof(int), "i");
        var j = Parameter(typeof(int), "j");
        var k = Parameter(typeof(int), "k");
        var l = Parameter(typeof(int), "l");

        AssertStatement(
            """
{
    int i;
    int k;
    var j = 8;
    switch (j)
    {
        case 100:
            i = 1;
            break;
        case 200:
        {
            switch (k)
            {
                case 200:
                {
                    var l = LinqToCSharpSyntaxTranslatorTest.Foo();
                    i = LinqToCSharpSyntaxTranslatorTest.ReturnsIntWithParam(l);
                    break;
                }

                case 300:
                    i = 3;
                    break;
                default:
                    i = 0;
                    break;
            }

            break;
        }

        default:
            i = 0;
            break;
    }
}
""", Block(
                variables: [i, j, k],
                Assign(j, Constant(8)),
                Assign(
                    i,
                    Switch(
                        j,
                        defaultBody: Constant(0),
                        SwitchCase(Constant(1), Constant(100)),
                        SwitchCase(
                            Switch(
                                k,
                                defaultBody: Constant(0),
                                SwitchCase(
                                    Block(
                                        variables: [l],
                                        Assign(l, Call(FooMethod)),
                                        Call(ReturnsIntWithParamMethod, l)),
                                    Constant(200)),
                                SwitchCase(Constant(3), Constant(300))),
                            Constant(200))))));
    }

    [Fact]
    public void Lift_non_literal_switch_expression()
    {
        var i = Parameter(typeof(int), "i");

        AssertStatement(
            """
{
    int i;
    if (blog1 == blog2)
    {
        LinqToCSharpSyntaxTranslatorTest.ReturnsIntWithParam(8);
        i = 1;
    }
    else
    {
        if (blog1 == blog3)
        {
            LinqToCSharpSyntaxTranslatorTest.ReturnsIntWithParam(9);
            i = 2;
        }
        else
        {
            i = blog1 == blog4 ? 3 : 0;
        }
    }
}
""", Block(
                variables: [i],
                Assign(
                    i,
                    Switch(
                        Parameter(typeof(Blog), "blog1"),
                        defaultBody: Block(Constant(0)),
                        SwitchCase(
                            Block(
                                Call(ReturnsIntWithParamMethod, Constant(8)),
                                Constant(1)),
                            Parameter(typeof(Blog), "blog2")),
                        SwitchCase(
                            Block(
                                Call(ReturnsIntWithParamMethod, Constant(9)),
                                Constant(2)),
                            Parameter(typeof(Blog), "blog3")),
                        SwitchCase(Constant(3), Parameter(typeof(Blog), "blog4"))))));
    }

    [Fact]
    public void ListInit_node()
        => AssertExpression(
            """
new List<int>()
{
    8,
    9
}
""", ListInit(
                New(typeof(List<int>)),
                typeof(List<int>).GetMethod(nameof(List<int>.Add))!,
                Constant(8),
                Constant(9)));

    [Fact]
    public void TypeEqual_node()
        => AssertExpression("p == typeof(int)", TypeEqual(Parameter(typeof(object), "p"), typeof(int)));

    [Fact]
    public void TypeIs_node()
        => AssertExpression("p is int", TypeIs(Parameter(typeof(object), "p"), typeof(int)));

    [Fact]
    public void Goto_with_named_label()
    {
        var labelTarget = Label("label1");

        AssertStatement(
            """
{
    goto label1;
    label1:
        LinqToCSharpSyntaxTranslatorTest.Foo();
}
""", Block(
                Goto(labelTarget),
                Label(labelTarget),
                Call(FooMethod)));
    }

    [Fact]
    public void Goto_with_label_on_last_line()
    {
        var labelTarget = Label("label1");

        AssertStatement(
            """
{
    goto label1;
    label1:
        ;
}
""", Block(
                Goto(labelTarget),
                Label(labelTarget)));
    }

    [Fact]
    public void Goto_outside_label()
    {
        var labelTarget = Label();

        AssertStatement(
            """
{
    if (true)
    {
        LinqToCSharpSyntaxTranslatorTest.Foo();
        goto unnamedLabel;
    }

    unnamedLabel:
        ;
}
""", Block(
                IfThen(
                    Constant(true),
                    Block(
                        Call(FooMethod),
                        Goto(labelTarget))),
                Label(labelTarget)));
    }

    [Fact]
    public void Goto_with_unnamed_labels_in_sibling_blocks()
    {
        var labelTarget1 = Label();
        var labelTarget2 = Label();

        AssertStatement(
            """
{
    {
        goto unnamedLabel;
        unnamedLabel:
            ;
    }

    {
        goto unnamedLabel;
        unnamedLabel:
            ;
    }
}
""", Block(
                Block(
                    Goto(labelTarget1),
                    Label(labelTarget1)),
                Block(
                    Goto(labelTarget2),
                    Label(labelTarget2))));
    }

    [Fact]
    public void Loop_statement_infinite()
        => AssertStatement(
            """
while (true)
{
    LinqToCSharpSyntaxTranslatorTest.Foo();
}
""", Loop(Call(FooMethod)));

    [Fact]
    public void Loop_statement_with_break_and_continue()
    {
        var i = Parameter(typeof(int), "i");
        var breakLabel = Label();
        var continueLabel = Label();

        AssertStatement(
            """
{
    var i = 0;
    {
        while (true)
        {
            unnamedLabel0:
                if (i == 100)
                {
                    goto unnamedLabel;
                }

            if (i % 2 == 0)
            {
                goto unnamedLabel0;
            }

            i++;
        }

        unnamedLabel:
            ;
    }
}
""", Block(
                variables: [i],
                Assign(i, Constant(0)),
                Loop(
                    Block(
                        IfThen(
                            Equal(i, Constant(100)),
                            Break(breakLabel)),
                        IfThen(
                            Equal(Modulo(i, Constant(2)), Constant(0)),
                            Continue(continueLabel)),
                        PostIncrementAssign(i)),
                    breakLabel,
                    continueLabel)));
    }

    [Fact]
    public void Try_catch_statement()
    {
        var e = Parameter(typeof(InvalidOperationException), "e");

        AssertStatement(
            """
try
{
    LinqToCSharpSyntaxTranslatorTest.Foo();
}
catch (InvalidOperationException e)
{
    LinqToCSharpSyntaxTranslatorTest.Bar();
}
catch (InvalidOperationException e)
{
    LinqToCSharpSyntaxTranslatorTest.Baz();
}
""", TryCatch(
                Call(FooMethod),
                Catch(e, Call(BarMethod)),
                Catch(e, Call(BazMethod))));
    }

    [Fact]
    public void Try_finally_statement()
        => AssertStatement(
            """
try
{
    LinqToCSharpSyntaxTranslatorTest.Foo();
}
finally
{
    LinqToCSharpSyntaxTranslatorTest.Bar();
}
""", TryFinally(
                Call(FooMethod),
                Call(BarMethod)));

    [Fact]
    public void Try_catch_finally_statement()
    {
        var e = Parameter(typeof(InvalidOperationException), "e");

        AssertStatement(
            """
try
{
    LinqToCSharpSyntaxTranslatorTest.Foo();
}
catch (InvalidOperationException e)
{
    LinqToCSharpSyntaxTranslatorTest.Bar();
}
catch (InvalidOperationException e)when (e.Message == "foo")
{
    LinqToCSharpSyntaxTranslatorTest.Baz();
}
finally
{
    LinqToCSharpSyntaxTranslatorTest.Bar();
    LinqToCSharpSyntaxTranslatorTest.Baz();
}
""", TryCatchFinally(
                Call(FooMethod),
                Block(
                    Call(BarMethod),
                    Call(BazMethod)),
                Catch(e, Call(BarMethod)),
                Catch(
                    e,
                    Call(BazMethod),
                    Equal(
                        Property(e, nameof(Exception.Message)),
                        Constant("foo")))));
    }

    [Fact]
    public void Try_catch_statement_with_filter()
    {
        var e = Parameter(typeof(InvalidOperationException), "e");

        AssertStatement(
            """
try
{
    LinqToCSharpSyntaxTranslatorTest.Foo();
}
catch (InvalidOperationException e)when (e.Message == "foo")
{
    LinqToCSharpSyntaxTranslatorTest.Bar();
}
""", TryCatch(
                Call(FooMethod),
                Catch(
                    e,
                    Call(BarMethod),
                    Equal(
                        Property(e, nameof(Exception.Message)),
                        Constant("foo")))));
    }

    [Fact]
    public void Try_catch_statement_without_exception_reference()
        => AssertStatement(
            """
try
{
    LinqToCSharpSyntaxTranslatorTest.Foo();
}
catch (InvalidOperationException)
{
    LinqToCSharpSyntaxTranslatorTest.Bar();
}
""", TryCatch(
                Call(FooMethod),
                Catch(
                    typeof(InvalidOperationException),
                    Call(BarMethod))));

    [Fact]
    public void Try_fault_statement()
        => AssertStatement(
            """
try
{
    LinqToCSharpSyntaxTranslatorTest.Foo();
}
catch
{
    LinqToCSharpSyntaxTranslatorTest.Bar();
}
""", TryFault(
                Call(FooMethod),
                Call(BarMethod)));

    // TODO: try/catch expressions

    private void AssertStatement(
        string expected,
        Expression expression,
        Dictionary<object, string>? constantReplacements = null,
        Dictionary<MemberAccess, string>? memberAccessReplacements = null)
        => AssertCore(expected, isStatement: true, expression: expression, constantReplacements: constantReplacements, memberAccessReplacements: memberAccessReplacements);

    private void AssertExpression(
        string expected,
        Expression expression,
        Dictionary<object, string>? constantReplacements = null,
        Dictionary<MemberAccess, string>? memberAccessReplacements = null)
        => AssertCore(expected, isStatement: false, expression: expression, constantReplacements: constantReplacements, memberAccessReplacements: memberAccessReplacements);

    private void AssertCore(
        string expected,
        bool isStatement,
        Expression expression,
        Dictionary<object, string>? constantReplacements,
        Dictionary<MemberAccess, string>? memberAccessReplacements)
    {
        var typeMappingSource = new SqlServerTypeMappingSource(
            TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
            new RelationalTypeMappingSourceDependencies([]));

        var translator = new CSharpHelper(typeMappingSource);
        var namespaces = new HashSet<string>();
        var actual = isStatement
            ? translator.Statement(expression, constantReplacements, memberAccessReplacements, namespaces)
            : translator.Expression(expression, constantReplacements, memberAccessReplacements, namespaces);

        if (_outputExpressionTrees)
        {
            testOutputHelper.WriteLine("---- Input LINQ expression tree:");
            testOutputHelper.WriteLine(_expressionPrinter.PrintExpression(expression));
        }

        // TODO: Actually compile the output C# code to make sure it's valid.
        // TODO: For extra credit, execute both code representations and make sure the results are the same

        try
        {
            Assert.Equal(expected, actual, ignoreLineEndingDifferences: true);

            if (_outputExpressionTrees)
            {
                testOutputHelper.WriteLine("---- Output Roslyn syntax tree:");
                testOutputHelper.WriteLine(actual);
            }
        }
        catch (EqualException)
        {
            testOutputHelper.WriteLine("---- Output Roslyn syntax tree:");
            testOutputHelper.WriteLine(actual);

            throw;
        }
    }

    private (LinqToCSharpSyntaxTranslator, AdhocWorkspace) CreateTranslator()
    {
        var workspace = new AdhocWorkspace();
        var syntaxGenerator = SyntaxGenerator.GetGenerator(workspace, LanguageNames.CSharp);
        return (new LinqToCSharpSyntaxTranslator(syntaxGenerator), workspace);
    }

    // ReSharper disable UnusedMember.Local
    // ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
    // ReSharper disable UnusedParameter.Local
    // ReSharper disable UnusedAutoPropertyAccessor.Local
    // ReSharper disable MemberCanBePrivate.Local

    private static readonly MethodInfo ReturnsIntWithParamMethod
        = typeof(LinqToCSharpSyntaxTranslatorTest).GetMethod(nameof(ReturnsIntWithParam))!;

    public static int ReturnsIntWithParam(int i)
        => i + 1;

    private static readonly MethodInfo WithInOutRefParameterMethod
        = typeof(LinqToCSharpSyntaxTranslatorTest).GetMethod(nameof(WithInOutRefParameter))!;

    public static void WithInOutRefParameter(in int inParam, out int outParam, ref int refParam)
        => outParam = 8;

    private static readonly MethodInfo GenericMethod
        = typeof(LinqToCSharpSyntaxTranslatorTest).GetMethods().Single(m => m.Name == nameof(GenericMethodImplementation));

    public static int GenericMethodImplementation<T>(T t)
        => 0;

    private static readonly MethodInfo FooMethod
        = typeof(LinqToCSharpSyntaxTranslatorTest).GetMethod(nameof(Foo))!;

    public static int Foo()
        => 1;

    private static readonly MethodInfo BarMethod
        = typeof(LinqToCSharpSyntaxTranslatorTest).GetMethod(nameof(Bar))!;

    public static int Bar()
        => 1;

    private static readonly MethodInfo BazMethod
        = typeof(LinqToCSharpSyntaxTranslatorTest).GetMethod(nameof(Baz))!;

    public static int Baz()
        => 1;

    public static int MethodWithSixParams(int a, int b, int c, int d, int e, int f)
        => a + b + c + d + e + f;


    private static readonly FieldInfo BlogPrivateField
        = typeof(Blog).GetField("_privateField", BindingFlags.NonPublic | BindingFlags.Instance)!;

    private class Blog
    {
#pragma warning disable CS0169
#pragma warning disable CS0649
        public int PublicField;
        public int PublicProperty { get; set; }
        internal int InternalField;
        internal int InternalProperty { get; set; }
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable IDE0051 // Remove unused private members
        private int _privateField;
#pragma warning restore IDE0051 // Remove unused private members
#pragma warning restore IDE0044 // Add readonly modifier
        private int PrivateProperty { get; set; }

        public List<int> ListOfInts { get; set; } = [];
        public BlogDetails Details { get; set; } = new();
#pragma warning restore CS0649
#pragma warning restore CS0169

        public Blog() { }
        public Blog(string name) { }
        public Blog(int foo, int bar) { }

        public int SomeInstanceMethod()
            => 3;

        public static readonly ConstructorInfo Constructor
            = typeof(Blog).GetConstructor([])!;

        public static int Static_method_on_nested_type()
            => 3;
    }

    public class BlogDetails
    {
        public int Foo { get; set; }
        public List<int> ListOfInts { get; set; } = [];
    }

    private class BlogWithRequiredProperties
    {
        public required string Name { get; set; }
        public required int Rating { get; set; }

        public BlogWithRequiredProperties() { }

        public BlogWithRequiredProperties(string name)
        {
            Name = name;
        }

        [SetsRequiredMembers]
        public BlogWithRequiredProperties(string name, int rating)
        {
            Name = name;
            Rating = rating;
        }
    }

    [Flags]
    public enum SomeEnum
    {
        One = 1,
        Two = 2
    }

    // ReSharper restore UnusedMember.Local
    // ReSharper restore AutoPropertyCanBeMadeGetOnly.Local
    // ReSharper restore UnusedParameter.Local
    // ReSharper restore UnusedAutoPropertyAccessor.Local
    // ReSharper restore MemberCanBePrivate.Local

    private readonly ExpressionPrinter _expressionPrinter = new();
    private readonly bool _outputExpressionTrees = true;
}

internal class LinqExpressionToRoslynTranslatorExtensionType
{
    public static readonly ConstructorInfo Constructor
        = typeof(LinqExpressionToRoslynTranslatorExtensionType).GetConstructor([])!;
}

internal static class LinqExpressionToRoslynTranslatorExtensions
{
    public static readonly MethodInfo SomeExtensionMethod
        = typeof(LinqExpressionToRoslynTranslatorExtensions).GetMethod(
            nameof(SomeExtension), [typeof(LinqExpressionToRoslynTranslatorExtensionType)])!;

    public static int SomeExtension(this LinqExpressionToRoslynTranslatorExtensionType? someType)
        => 3;
}
