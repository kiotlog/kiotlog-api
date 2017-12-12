module Kiotlog.Web.API

open Suave
open System.Net

[<EntryPoint>]
let main argv =
    let cs = "Username=postgres;Password=;Host=127.0.0.1;Port=7433;Database=trmpln"

    let app =
        choose [
            Webparts.Devices.webPart cs
            RequestErrors.NOT_FOUND "Found no handlers"
        ]

    let conf = { defaultConfig with bindings = [HttpBinding.create HTTP IPAddress.Loopback 8888us] }
    startWebServer conf app

    0 // return an integer exit code
