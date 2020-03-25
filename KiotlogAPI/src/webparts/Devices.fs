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

module Kiotlog.Web.Webparts.Devices

open System
open System.Collections.Generic
open System.Linq
open Suave
open Microsoft.EntityFrameworkCore

open Kiotlog.Web.RestFul
open Kiotlog.Web.DB
open Kiotlog.Web.Railway
open Kiotlog.Web.Utils
open Kiotlog.Web.Webparts.Generics

open KiotlogDBF.Models
open KiotlogDBF.Context
open Suave.Successful
open Kiotlog.Web.Json
open System.Text
open Suave.Filters
open Suave.Operators
open Newtonsoft.Json.Linq
open Microsoft.EntityFrameworkCore.Query


let getDevicesAsync (cs : string) () =
    async {
        use ctx = getContext cs

        try
            let now = DateTime.UtcNow
            let devicesWithAnnotations =
                ctx.Devices
                    .Include(fun d -> d.Annotations :> IEnumerable<_>)
                    
            let! devices = devicesWithAnnotations.ToArrayAsync() |> Async.AwaitTask

            return Ok (
                    devices
                    |> Array.map(fun d ->
                        d.Annotations <- d.Annotations.Where(fun x -> x.Begin < now && (not x.End.HasValue || x.End.Value > now)).OrderByDescending(fun a -> a.Begin).Take(1).ToHashSet()
                        d
                    )
                )
        with | _ -> return Error { Errors = [|"Error getting devices from DB"|]; Status = HTTP_500 }
    }

let getDevices (cs : string) () =
    getDevicesAsync cs () |> Async.RunSynchronously

let createDevice (cs : string) (device: Devices) =
    use ctx = getContext cs

    device |> ctx.Devices.Add |> ignore

    try
        ctx.SaveChanges() |> ignore
        Ok device
    with
    | :? DbUpdateException -> Error { Errors = [|"Error adding Device"|]; Status = HTTP_409 }
    | _ -> Error { Errors = [|"Some DB error occurred"|]; Status = HTTP_500 }

let querybyId (deviceId : Guid) (devices: IIncludableQueryable<Devices,Conversions>) =
    query {
        for d in devices do
        where (d.Id = deviceId)
        select d
    }

let querybyName (device : String) (devices: IIncludableQueryable<Devices,Conversions>) =
    query {
        for d in devices do
        where (d.Device = device)
        select d
    }

let private loadDeviceWithSensorsAndAnnotationsAsync (ctx : KiotlogDBFContext) (queryBy: IIncludableQueryable<Devices,Conversions> -> IQueryable<Devices>) =
    async {
        try
            let devices =
                ctx.Devices
                    .Include(fun d -> d.Sensors :> IEnumerable<_>)
                        .ThenInclude(fun (s : Sensors) -> s.SensorType)
                    .Include(fun d -> d.Sensors :> IEnumerable<_>)
                        .ThenInclude(fun (s : Sensors) -> s.Conversion)
                    // .Include(fun d -> d.Annotations :> IEnumerable<_>)
                    // .Include("Sensors.SensorType")
                    // .Include("Sensors.Conversion")

            let q = queryBy devices 
            
            let! device = q.SingleOrDefaultAsync () |> Async.AwaitTask
            match box device with
            | null -> return Error { Errors = [|"Device not found"|]; Status = HTTP_404 }
            | d ->
                let now = DateTime.UtcNow
                let annotation =
                    query {
                        for a in ctx.Annotations do
                        where (a.DeviceId = device.Id && a.Begin < now && (not a.End.HasValue || a.End.Value > now))
                        sortByDescending a.Begin
                        take 1
                    }            
                device.Annotations = annotation.ToHashSet() |> ignore
                return Ok (d :?> Devices)
        with
        | _ -> return Error { Errors = [|"Some DB error occurred"|]; Status = HTTP_500 }
    }

let getDeviceByIdAsync (cs : string) (deviceId : Guid) =
    async {
        use ctx = getContext cs

        return! loadDeviceWithSensorsAndAnnotationsAsync ctx (querybyId deviceId)
    }

let getDeviceById (cs : string) (deviceId: Guid) =
    getDeviceByIdAsync cs deviceId |> Async.RunSynchronously

let getDeviceByNameAsync (cs : string) (device : String) =
    async {
        use ctx = getContext cs

        return! loadDeviceWithSensorsAndAnnotationsAsync ctx (querybyName device)
    }

let getDeviceByName (cs : string) (device: String) =
    getDeviceByNameAsync cs device |> Async.RunSynchronously

let updateDeviceByIdAsync (cs : string) (deviceId: Guid) (device: Devices) =
    async {
        use ctx = getContext cs

        device.Id <- deviceId

        let! res = loadDeviceWithSensorsAndAnnotationsAsync ctx (querybyId deviceId)

        match res with
        | Error _ -> return res
        | Ok entity ->
            if not (String.IsNullOrEmpty device.Device) then entity.Device <- device.Device
            if not (isNull (box device.Auth)) then entity.Auth <- device.Auth
            if not (isNull (box device.Frame)) then entity.Frame <- entity.Frame

            if not (isNull device.Sensors) && device.Sensors.Count > 0 then
                let updateSensor = fun (s : Sensors) ->
                    let existing =
                        let f =
                            query {
                                for x in entity.Sensors do
                                where (x.Id = s.Id)
                                select x
                                exactlyOneOrDefault
                            } // entity.Sensors.SingleOrDefault(fun x -> x.Id = s.Id)
                        match box f with
                        | null ->
                            entity.Sensors.Add s |> ignore
                            s
                        | _ -> f

                    existing |> ctx.Sensors.Attach |> ignore
                    if not (isNull s.Meta) then existing.Meta <- s.Meta
                        // ctx.Entry(existing).Property("_Meta").IsModified <- true
                    // if s.ConversionId. then
                    existing.ConversionId <- s.ConversionId
                    // if s.SensorTypeId.HasValue then
                    existing.SensorTypeId <- s.SensorTypeId
                    if not (isNull s.Fmt) then existing.Fmt <- s.Fmt

                device.Sensors |> Seq.iter updateSensor

            try
                ctx.SaveChanges() |> ignore
                return Ok entity
            with
            | :? DbUpdateException -> return Error { Errors = [|"Error updating Device"|]; Status = HTTP_409 }
            | _ -> return Error { Errors = [|"Some DB error occurred"|]; Status = HTTP_500 }
    }

let updateDeviceById (cs : string) (deviceId: Guid) (device: Devices) =
    updateDeviceByIdAsync cs deviceId device |> Async.RunSynchronously

let patchDeviceById (cs : string) (annotatioId: Guid) (annotation: JObject) : Result<Devices, RestError> =
    Error { Errors = [|"will be implemented"|]; Status = HTTP_501 }

let deleteDeviceAsync (cs : string) (deviceId : Guid) =
    async {
        use ctx = getContext cs

        let! device = ctx.Devices.FindAsync(deviceId).AsTask () |> Async.AwaitTask

        match box device with
        | null -> return Error { Errors = [|"Device not found"|]; Status = HTTP_404}
        | d ->
            d :?> Devices |> ctx.Devices.Remove |> ignore

            try
                ctx.SaveChanges() |> ignore
                return Ok ()
            with
            | :? DbUpdateException -> return Error { Errors = [|"Error adding Device"|]; Status = HTTP_409 }
            | _ -> return Error { Errors = [|"Some DB error occurred"|]; Status = HTTP_500 }
    }

let deleteDevice (cs : string) (deviceId : Guid) =
    deleteDeviceAsync cs deviceId |> Async.RunSynchronously


// let handleRailwayResource = function
//     | Ok x -> JSON OK x
//     | Error e -> JSON (Encoding.UTF8.GetBytes >> Response.response e.Status) e

// let getResourceById =
//     getDevice >> handleRailwayResource

// let resourceIdPath = new PrintfFormat<(Guid -> string),unit,string,string,Guid>(resourcePath + "/%s")


let private getAnnotationsByDeviceAsync (cs : string) (deviceId : Guid) =
    async {
        use ctx = getContext cs

        try
            let devices =
                ctx.Devices
                    .Include(fun d -> d.Annotations :> IEnumerable<_>)

            let q =
                query {
                    for d in devices do
                    where (d.Id = deviceId)
                    select d
                }
            let! device = q.SingleOrDefaultAsync () |> Async.AwaitTask

            match box device with
            | null -> return Error { Errors = [|"Device not found"|]; Status = HTTP_404 }
            | d -> return Ok ((d :?> Devices).Annotations.OrderByDescending(fun a -> a.Begin))
        with
        | _ -> return Error { Errors = [|"Some DB error occurred"|]; Status = HTTP_500 }
    }

let createAnnotation (cs : string) (devideId : Guid) (annotation : Annotations) =
    annotation.DeviceId <- devideId
    createEntity<Annotations> cs annotation

let private annotations (cs : string) (deviceId : Guid) =
    choose [
        GET >=> (getAnnotationsByDeviceAsync cs deviceId |> Async.RunSynchronously |> handleRailwayResource)
        POST >=> request (getResourceFromReq >> Result.bind validate >> Result.bind (createAnnotation cs deviceId) >> handleRailwayResource)
    ]

let webPart (cs : string) =
    choose [
        // pathScan "/devices/%s/annotations" (Guid.Parse >> annotations cs)
        regexPatternRouting ("/devices/" + uuidRegEx + "/annotations") (uuidMatcher (annotations cs))

        rest {
            Name = "devices"
            GetAll = getDevices cs
            Create = createDevice cs
            Delete = deleteDevice cs
            GetById = getDeviceById cs
            UpdateById = updateDeviceById cs
            PatchById = patchDeviceById cs
        }

        GET >=> pathScan "/devices/%s" (getDeviceByName cs >> handleRailwayResource)
    ]
