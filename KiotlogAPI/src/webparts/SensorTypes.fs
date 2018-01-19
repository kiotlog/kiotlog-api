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

module Kiotlog.Web.Webparts.SensorTypes

open System
open Suave
open Kiotlog.Web.RestFul
open Kiotlog.Web.DB
open Kiotlog.Web.Railway

open KiotlogDB
open Microsoft.EntityFrameworkCore

let getSensorTypesAsync (cs : string) () =
    async {
        use ctx = getContext cs

        try
            let! sensortypes = ctx.SensorTypes.ToArrayAsync() |> Async.AwaitTask

            return Ok sensortypes
        with | _ -> return Error { Errors = [|"Error getting Sensort Types from DB"|]; Status = HTTP_500 }
    }

let getSensorTypes (cs : string) () =
    getSensorTypesAsync cs () |> Async.RunSynchronously

let createSensorType (cs : string) (sensortype: SensorTypes) =
    use ctx = getContext cs

    sensortype |> ctx.SensorTypes.Add |> ignore

    try
        ctx.SaveChanges() |> ignore
        Ok sensortype
    with
    | :? DbUpdateException -> Error { Errors = [|"Error adding SensorType"|]; Status = HTTP_409 }
    | _ -> Error { Errors = [|"Some DB error occurred"|]; Status = HTTP_500 }

let private loadSensorTypeAsync (ctx : KiotlogDBContext) (sensortypeId : Guid) =
    async {
        try
            let sensortypes = ctx.SensorTypes
            let q =
                query {
                    for st in sensortypes do
                    where (st.Id = sensortypeId)
                    select st
                }
            let! sensortype = q.SingleOrDefaultAsync () |> Async.AwaitTask

            match sensortype with
            | null -> return Error { Errors = [|"Sensor Type not found"|]; Status = HTTP_404 }
            | d -> return Ok d
        with
        | _ -> return Error { Errors = [|"Some DB error occurred"|]; Status = HTTP_500 }

    }

let getSensorTypeAsync (cs : string) (sensortypeId : Guid) =
    async {
        use ctx = getContext cs

        return! loadSensorTypeAsync ctx sensortypeId
    }

let getSensorType (cs : string) (sensortypeId: Guid) =
    getSensorTypeAsync cs sensortypeId |> Async.RunSynchronously

let updateSensorTypeByIdAsync (cs : string) (sensortypeId: Guid) (sensortype: SensorTypes) =
    async {
        use ctx = getContext cs

        sensortype.Id <- sensortypeId

        let! res = loadSensorTypeAsync ctx sensortypeId

        match res with
        | Error _ -> return res
        | Ok entity ->
            if not (String.IsNullOrEmpty sensortype.Name) then entity.Name <- sensortype.Name
            if not (isNull sensortype.Meta) then entity.Meta <- sensortype.Meta
            if not (isNull sensortype.Type) then entity.Type <- entity.Type

            try
                ctx.SaveChanges() |> ignore
                return Ok entity
            with
            | :? DbUpdateException -> return Error { Errors = [|"Error updating SensorType"|]; Status = HTTP_409 }
            | _ -> return Error { Errors = [|"Some DB error occurred"|]; Status = HTTP_500 }
    }
let updateSensorTypeById (cs : string) (sensortypeId: Guid) (sensortype: SensorTypes) =
    updateSensorTypeByIdAsync cs sensortypeId sensortype |> Async.RunSynchronously

let deleteSensorTypeAsync (cs : string) (sensortypeId : Guid) =
    async {
        use ctx = getContext cs

        let! sensortype = ctx.SensorTypes.FindAsync(sensortypeId) |> Async.AwaitTask

        match sensortype with
        | null -> return Error { Errors = [|"SensorType not found"|]; Status = HTTP_404}
        | d ->
            d |> ctx.SensorTypes.Remove |> ignore

            try
                ctx.SaveChanges() |> ignore
                return Ok ()
            with
            | :? DbUpdateException -> return Error { Errors = [|"Error adding SensorType"|]; Status = HTTP_409 }
            | _ -> return Error { Errors = [|"Some DB error occurred"|]; Status = HTTP_500 }
    }

let deleteSensorType (cs : string) (sensortypeId : Guid) =
    deleteSensorTypeAsync cs sensortypeId |> Async.RunSynchronously

let webPart (cs : string) =
    choose [
        rest {
            Name = "sensortypes"
            GetAll = getSensorTypes cs
            Create = createSensorType cs
            Delete = deleteSensorType cs
            GetById = getSensorType cs
            UpdateById = updateSensorTypeById cs
        }
    ]
