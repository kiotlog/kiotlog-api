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

module Kiotlog.Web.Webparts.Status

open System
open System.Collections.Generic
open System.Linq
open Suave
open Suave.Operators
open Suave.Filters
open Suave.Successful

open Kiotlog.Web.DB
open Kiotlog.Web.Json


type StatusDevice = {
    Id : Guid
    Device : string
    LastPoint : DateTime Nullable
}

type StatusSensorTypeGroup = {
    Type : string
    Count : int
}

type StatusSensors = {
    Total : int
    Types : IEnumerable<StatusSensorTypeGroup>
}

type Status = {
   Devices : IEnumerable<StatusDevice>
   Sensors : StatusSensors
}

let private getStatus (cs : string) =
    let ctx = getContext cs

    let qDevices = query {
        for d in ctx.Devices do
        select (
            {
                Id = d.Id
                Device = d.Device;
                LastPoint = query {
                    for p in d.Points do
                    maxByNullable (Nullable p.Time)
                }
            }
        )
    }
    
    let qSensors = query {
        for s in ctx.Sensors do
        join t in ctx.SensorTypes
            on (s.SensorTypeId = t.Id)
        groupBy t.Type into g
        select { Type = g.Key; Count = g.Count() }
    }

    let makeTotal s =
        Seq.sumBy (fun x -> x.Count) s 

    let status = {
        Devices = qDevices;
        Sensors = { Total = makeTotal qSensors ; Types = qSensors }
    }

    JSON OK status


let webPart (cs : string) =
    choose [
        path "/status" >=> choose [
            GET >=> warbler (fun _ -> getStatus cs)
        ]
    ]