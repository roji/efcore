// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

#nullable enable

public abstract class ComplexTypeQueryTestBase<TFixture> : QueryTestBase<TFixture>
    where TFixture : ComplexTypeQueryTestBase<TFixture>.ComplexTypeQueryFixtureBase, new()
{
    public ComplexTypeQueryTestBase(TFixture fixture)
        : base(fixture)
    {
        fixture.ListLoggerFactory.Clear();
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_on_property_inside_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.ShippingAddress.ZipCode == 07728));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_on_property_inside_nested_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.ShippingAddress.Country.Code == "DE"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_on_property_inside_complex_type_after_subquery(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .OrderBy(c => c.Id)
                .Skip(1)
                .Distinct()
                .Where(c => c.ShippingAddress.ZipCode == 07728));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Filter_on_property_inside_nested_complex_type_after_subquery(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .OrderBy(c => c.Id)
                .Skip(1)
                .Distinct()
                .Where(c => c.ShippingAddress.Country.Code == "DE"));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Load_complex_type_after_subquery_on_entity_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .OrderBy(c => c.Id)
                .Skip(1)
                .Distinct());

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => c.ShippingAddress));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_nested_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => c.ShippingAddress.Country));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_single_property_on_nested_complex_type(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<Customer>().Select(c => c.ShippingAddress.Country.FullName));

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_multiplex_complex_types(bool async)
        => throw new NotImplementedException();

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Project_same_complex_type_twice(bool async)
        => throw new NotImplementedException();

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Impossible_equality_operator(bool async)
        => throw new NotImplementedException();

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Impossible_equality_method(bool async)
        => throw new NotImplementedException();

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Impossible_object_equality_method(bool async)
        => throw new NotImplementedException();

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projecting_base_type_loads_all_owned_navs(bool async)
        => throw new NotImplementedException();

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Set_throws_for_owned_type(bool async)
        => throw new NotImplementedException();

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Entity_splitting_with_complex_property_in_own_table(bool async)
        => throw new NotImplementedException();

    // TODO: Got to Preserve_includes_when_applying_skip_take_after_anonymous_type_select in OwnedQueryTestBase

    // TODO: Query filter going into complex type

    // TODO: Tracking queries

    // TODO: Exercise all inheritance scenarios, with the complex property being on derived types.

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Projecting_complex_type_in_tracking_query_throws(bool async)
        => throw new NotImplementedException();

    public abstract class ComplexTypeQueryFixtureBase : SharedStoreFixtureBase<PoolableDbContext>, IQueryFixtureBase
    {
        protected override string StoreName
            => "ComplexTypeQueryTest";

        private ComplexTypeQueryData? _expectedData;

        public override PoolableDbContext CreateContext()
        {
            var context = base.CreateContext();
            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            return context;
        }

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(wcb => wcb.Throw());

        protected override void Seed(PoolableDbContext context)
            => ComplexTypeQueryData.Seed(context);

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Entity<Customer>(cb =>
            {
                cb.Property(c => c.Id).ValueGeneratedNever();

                cb.ComplexProperty(c => c.ShippingAddress, sab => sab.ComplexProperty(sa => sa.Country));
            });
        }

        public Func<DbContext> GetContextCreator()
            => () => CreateContext();

        public ISetSource GetExpectedData()
            => _expectedData ??= new ComplexTypeQueryData();

        public IReadOnlyDictionary<Type, object> EntitySorters { get; } = new Dictionary<Type, Func<object, object?>>
        {
            { typeof(Customer), e => ((Customer)e).Id },

            // Complex types - still need comparers for cases where they are projected directly
            { typeof(Address), e => ((Address)e).ZipCode },
            { typeof(Country), e => ((Country)e).Code }
        }.ToDictionary(e => e.Key, e => (object)e.Value);

        public IReadOnlyDictionary<Type, object> EntityAsserters { get; } = new Dictionary<Type, Action<object, object>>
        {
            {
                typeof(Customer), (e, a) =>
                {
                    Assert.Equal(e == null, a == null);
                    if (a is not null && e is not null)
                    {
                        var ee = (Customer)e;
                        var aa = (Customer)a;

                        Assert.Equal(ee.Id, aa.Id);
                        Assert.Equal(ee.Name, aa.Name);
                        AssertAddress(ee.ShippingAddress, aa.ShippingAddress);
                    }
                }
            },
            {
                typeof(Address), (e, a) =>
                {
                    Assert.Equal(e == null, a == null);
                    if (a is not null && e is not null)
                    {
                        AssertAddress((Address)e, (Address)e);
                    }
                }
            },
            {
                typeof(Country), (e, a) =>
                {
                    Assert.Equal(e == null, a == null);
                    if (a is not null && e is not null)
                    {
                        AssertCountry((Country)e, (Country)e);
                    }
                }
            }
        }.ToDictionary(e => e.Key, e => (object)e.Value);
    }

    private static void AssertAddress(Address? expected, Address? actual)
    {
        if (expected is not null && actual is not null)
        {
            Assert.Equal(expected.AddressLine1, actual.AddressLine1);
            Assert.Equal(expected.AddressLine2, actual.AddressLine2);
            Assert.Equal(expected.ZipCode, actual.ZipCode);

            AssertCountry(expected.Country, actual.Country);
        }
        else
        {
            Assert.Equal(expected is null, actual is null);
        }
    }

    private static void AssertCountry(Country? expected, Country? actual)
    {
        if (expected is not null && actual is not null)
        {
            Assert.Equal(expected.FullName, actual.FullName);
            Assert.Equal(expected.Code, actual.Code);
        }
        else
        {
            Assert.Equal(expected is null, actual is null);
        }
    }

    protected class ComplexTypeQueryData : ISetSource
    {
        private readonly IReadOnlyList<Customer> _customers;

        public ComplexTypeQueryData()
        {
            _customers = CreateCustomers();

            // WireUp(_ownedPeople, _planets, _stars, _moons, _finks, _bartons);
        }

        public IQueryable<TEntity> Set<TEntity>()
            where TEntity : class
        {
            if (typeof(TEntity) == typeof(Customer))
            {
                return (IQueryable<TEntity>)_customers.AsQueryable();
            }

            throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity));
        }

        private static IReadOnlyList<Customer> CreateCustomers()
        {
            var customer1 = new Customer
            {
                Id = 1,
                Name = "Mona Cy",
                ShippingAddress = new Address
                {
                    AddressLine1 = "804 S. Lakeshore Road",
                    ZipCode = 38654,
                    Country = new Country
                    {
                        FullName = "United States",
                        Code = "US"
                    }
                }
            };

            var customer2 = new Customer
            {
                Id = 2,
                Name = "Antigonus Mitul",
                ShippingAddress = new Address
                {
                    AddressLine1 = "72 Hickory Rd.",
                    ZipCode = 07728,
                    Country = new Country
                    {
                        FullName = "Germany",
                        Code = "DE"
                    }
                }
            };

            return new List<Customer> { customer1, customer2 };
        }

        public static void Seed(PoolableDbContext context)
        {
            // TODO: Temporarily seeding via raw SQL as update pipeline support for complex types, see provider-specific implementations

            // context.Set<Customer>().AddRange(CreateCustomers());
            // context.SaveChanges();
        }
    }

    protected class Customer
    {
        public int Id { get; set; }
        public required string Name { get; set; }

        public required Address ShippingAddress { get; set; }

        // public ICollection<Order> Orders { get; set; }
    }

    protected class Address
    {
        public required string AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public int ZipCode { get; set; }

        public required Country Country { get; set; }
    }

    protected class Country
    {
        public required string FullName { get; set; }
        public required string Code { get; set; }
    }
}
