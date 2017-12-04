module Kiotlog.Web.API

open Suave
open System.Net

[<EntryPoint>]
let main argv =
    let app = choose [
                        Webparts.Devices.webPart;]

    let conf = {defaultConfig with bindings = [HttpBinding.create HTTP IPAddress.Loopback 8888us]}
    startWebServer conf app
    
    0 // return an integer exit code
