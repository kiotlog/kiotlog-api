module Kiotlog.Web.Webparts.Devices

open System
open Suave
open Kiotlog.Web.RestFul
open Kiotlog.Web.DB
open Kiotlog.Web.Railway

open Microsoft.EntityFrameworkCore
open KiotlogDB


let getDevicesAsync (cs : string) () =
    async {
        let ctx = getContext cs

        try
            let! devices = ctx.Devices.ToArrayAsync() |> Async.AwaitTask

            return Ok devices
        with | _ -> return Error { Errors = [|"Error getting devices from DB"|]; Status = HTTP_500 }
    }

let getDevices (cs : string) () =
    getDevicesAsync cs () |> Async.RunSynchronously

let createDevice (cs : string) (device: Devices) =
    let ctx = getContext cs
    // TODO: validate

    device |> ctx.Devices.Add |> ignore

    try
        ctx.SaveChanges() |> ignore
        Ok device
    with
    | :? DbUpdateException -> Error { Errors = [|"Error adding Device"|]; Status = HTTP_409 }
    | _ -> Error { Errors = [|"Some DB error occurred"|]; Status = HTTP_500 }

let getDeviceAsync (cs : string) (deviceId : Guid) =
    async {
        use ctx = getContext cs

        try
            let! device = ctx.Devices
                            // .FindAsync(deviceId)
                            .Include("Sensors")
                            .Include("Sensors.SensorType")
                            .SingleOrDefaultAsync(fun x -> x.Id.Equals(deviceId))
                            |> Async.AwaitTask

            match device with
            | null -> return Error { Errors = [|"Device not found"|]; Status = HTTP_404}
            | d -> return Ok d
        with
        | _ -> return Error { Errors = [|"Some DB error occurred"|]; Status = HTTP_500 }
    }

let getDevice (cs : string) (deviceId: Guid) =
    getDeviceAsync cs deviceId |> Async.RunSynchronously
    // use ctx = getContext cs
    // let devices = ctx.Devices
    //                 .Include("Sensors")
    //                 .Include("Sensors.SensorType")
    // let q = query {
    //             for d in devices do
    //             where (d.Id = deviceId)
    //             select d
    //             exactlyOneOrDefault
    //         }
    // match q with
    // | null -> None
    // | d -> Some d

let updateDeviceById (deviceId: Guid) (device: Devices) =
    Ok device

let deleteDeviceAsync (cs : string) (deviceId : Guid) =
    async {
        use ctx = getContext cs

        let! device = ctx.Devices.FindAsync(deviceId) |> Async.AwaitTask

        match device with
        | null -> return Error { Errors = [|"Device not found"|]; Status = HTTP_404}
        | d ->
            d |> ctx.Devices.Remove |> ignore

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
            //Update = Db_Dictionary.updatePerson
            Delete = deleteDevice cs
            GetById = getDevice cs
            UpdateById = updateDeviceById
            //IsExists = Db_Dictionary.isPersonExists
        }
    ]
