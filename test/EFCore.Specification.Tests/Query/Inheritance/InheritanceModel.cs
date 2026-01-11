// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Inheritance;

public class Root
{
    public int Id { get; set; }

    // The actual Id above might be database-generated, since we want to test database-generation with inheritance hierarchies;
    // so it's not suitable for comparing in-memory to database objects.
    // The following is a substitute unique identifier that is always set in the test data.
    public int UniqueId { get; set; }

    public int RootInt { get; set; }

    public ComplexType? ParentComplexType { get; set; }
    public List<ComplexType> ComplexTypeCollection { get; set; } = [];

    public RootReferencingEntity? RootReferencingEntity { get; set; }
    public int? RootReferencingEntityId { get; set; }
}

public class Intermediate : Root
{
    public int IntermediateInt { get; set; }
}

public class Leaf1 : Intermediate
{
    public int Leaf1Int { get; set; }
    public int[]? Ints { get; set; }

    // Same property name as on Leaf2, to test uniquification
    public ComplexType? ChildComplexType { get; set; }
}

public class Leaf2 : Intermediate
{
    public int Leaf2Int { get; set; }

    // Same property name as on Leaf1, to test uniquification
    public ComplexType? ChildComplexType { get; set; }
}

public class ConcreteIntermediate : Root
{
    public int ConcreteIntermediateInt { get; set; }
}

public class Leaf3 : ConcreteIntermediate
{
    public int Leaf3Int { get; set; }
}

public class ComplexType
{
    // For sorting when projecting out the complex type - must be unique
    public int UniqueId { get; set; }

    public int Int { get; set; }
    public NestedComplexType? Nested { get; set; }
}

public class NestedComplexType
{
    // For sorting when projecting out the complex type - must be unique
    public int UniqueId { get; set; }

    public int Int { get; set; }
}

/// <summary>
///     An external entity type not inside the hierarchy but referencing it, to exercise operations that target hierarchies via
///     navigations.
/// </summary>
public class RootReferencingEntity
{
    public int Id { get; set; }
    public int Int { get; set; }

    public Root? Root { get; set; }
}
