module Kiotlog.Web.DB

open Microsoft.EntityFrameworkCore
open KiotlogDB

let getContext() =
    let optionsBuilder = DbContextOptionsBuilder<KiotlogDBContext>()
    optionsBuilder.UseNpgsql("Username=postgres;Password=;Host=127.0.0.1;Port=7432;Database=trmpln") |> ignore
    
    new KiotlogDBContext(optionsBuilder.Options)