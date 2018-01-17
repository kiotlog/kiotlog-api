module Kiotlog.Web.API

open Suave
open Arguments

[<EntryPoint>]
let main argv =

    let config = parseCLI argv
    let cs = config.PostgresConnectionString

    let app =
        choose [
            Webparts.Devices.webPart cs
            RequestErrors.NOT_FOUND "Found no handlers"
        ]

    let conf = { defaultConfig with bindings = [HttpBinding.createSimple HTTP config.HttpHost config.HttpPort] }
    startWebServer conf app

    0 // return an integer exit code
