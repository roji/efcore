open System
open System.Linq
open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.Logging

#nowarn "20"

type Event() =
    member val Id: Guid = Guid.Empty with get, set
    member val Data: int = 0 with get, set

type ApplicationDbContext() =
    inherit DbContext()

    override this.OnConfiguring(builder) =
        builder
            .UseSqlServer(@"Server=localhost;Database=test;User=SA;Password=Abcd5678;Connect Timeout=60;ConnectRetryCount=0;Encrypt=false")
            .LogTo(Action<string>(Console.WriteLine), LogLevel.Information)
            .EnableSensitiveDataLogging() |> ignore

    [<DefaultValue>]
    val mutable private events: DbSet<Event>

    member this.Events
        with public get () = this.events
        and public set v = this.events <- v

[<EntryPoint>]
let main args =
    use db = new ApplicationDbContext()

    db.Database.EnsureDeleted()
    db.Database.EnsureCreated()

    let i = 8
    db.Events.Where(fun x -> x.Data = i).ToList()

    0
