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
open Suave
open Kiotlog.Web.RestFul
open Kiotlog.Web.DB
open Kiotlog.Web.Railway

open KiotlogDBF
open Microsoft.EntityFrameworkCore

let getDevicesAsync (cs : string) () =
    async {
        use ctx = getContext cs

        try
            let! devices = ctx.Devices.ToArrayAsync() |> Async.AwaitTask

            return Ok devices
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

let private loadDeviceWithSensorsAsync (ctx : KiotlogDBFContext) (deviceId : Guid) =
    async {
        try
            let devices =
                ctx.Devices
                    .Include("Sensors.SensorType")
                    .Include("Sensors.Conversion")

            let q =
                query {
                    for d in devices do
                    where (d.Id = deviceId)
                    select d
                }
            let! device = q.SingleOrDefaultAsync () |> Async.AwaitTask

            match box device with
            | null -> return Error { Errors = [|"Device not found"|]; Status = HTTP_404 }
            | d -> return Ok (d :?> Devices)
        with
        | _ -> return Error { Errors = [|"Some DB error occurred"|]; Status = HTTP_500 }
    }

let getDeviceAsync (cs : string) (deviceId : Guid) =
    async {
        use ctx = getContext cs

        return! loadDeviceWithSensorsAsync ctx deviceId
    }

let getDevice (cs : string) (deviceId: Guid) =
    getDeviceAsync cs deviceId |> Async.RunSynchronously

let updateDeviceByIdAsync (cs : string) (deviceId: Guid) (device: Devices) =
    async {
        use ctx = getContext cs

        device.Id <- deviceId

        let! res = loadDeviceWithSensorsAsync ctx deviceId

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
                            entity.Sensors.Add s
                            s
                        | _ -> f

                    existing |> ctx.Sensors.Attach |> ignore
                    if not (isNull (box s.Meta)) then existing.Meta <- s.Meta
                        // ctx.Entry(existing).Property("_Meta").IsModified <- true
                    // if s.ConversionId. then
                    existing.ConversionId <- s.ConversionId
                    // if s.SensorTypeId.HasValue then
                    existing.SensorTypeId <- s.SensorTypeId
                    if not (isNull (box s.Fmt)) then existing.Fmt <- s.Fmt

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

let deleteDeviceAsync (cs : string) (deviceId : Guid) =
    async {
        use ctx = getContext cs

        let! device = ctx.Devices.FindAsync(deviceId) |> Async.AwaitTask

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

let webPart (cs : string) =
    choose [
        rest {
            Name = "devices"
            GetAll = getDevices cs
            Create = createDevice cs
            Delete = deleteDevice cs
            GetById = getDevice cs
            UpdateById = updateDeviceById cs
        }
    ]
