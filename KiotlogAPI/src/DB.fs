module Kiotlog.Web.DB

open Microsoft.EntityFrameworkCore
open KiotlogDB

let getContext (cs : string) =
    let optionsBuilder = DbContextOptionsBuilder<KiotlogDBContext>()
    // let cs = "Username=postgres;Password=;Host=127.0.0.1;Port=7432;Database=trmpln"
    optionsBuilder.UseNpgsql(cs) |> ignore
    
    new KiotlogDBContext(optionsBuilder.Options)