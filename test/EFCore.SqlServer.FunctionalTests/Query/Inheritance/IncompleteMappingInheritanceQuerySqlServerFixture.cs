// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.Inheritance.TPH;

namespace Microsoft.EntityFrameworkCore.Query.Inheritance;

public class IncompleteMappingInheritanceQuerySqlServerFixture : TPHInheritanceQuerySqlServerFixture
{
    public override bool IsDiscriminatorMappingComplete
        => false;
}
