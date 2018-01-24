(*
    Copyright (C) 2017 Giampaolo Mancini, Trampoline SRL.
    Copyright (C) 2017 Francesco Varano, Trampoline SRL.

    This file is part of Kiotlog.

    Kiotlog is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Kiotlog is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*)

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
            Webparts.SensorTypes.webPart cs
            Webparts.Sensors.webPart cs
            Webparts.Conversions.webPart cs
            RequestErrors.NOT_FOUND "Found no handlers"
        ]

    let conf =
        { defaultConfig with
            bindings =
                [
                    HttpBinding.createSimple HTTP config.HttpHost config.HttpPort
                ]
        }
    startWebServer conf app

    0 // return an integer exit code
