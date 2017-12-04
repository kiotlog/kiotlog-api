module Kiotlog.Web.Webparts.Devices

open Suave
open Kiotlog.Web.RestFul
open Kiotlog.Web.DB
open KiotlogDB
open System
// open KiotlogDB
// open System.Linq
// open Microsoft.EntityFrameworkCore

let getDevices () =
    let ctx = getContext()
    ctx.Devices |> seq

let createDevice (device: Devices) =
    device

let deleteDevice (deviceId: Guid) =
    ()

let getDeviceAsync (deviceId: Guid) =
    async {
        use ctx = getContext()
        let! task = ctx.Devices.FindAsync(deviceId) |> Async.AwaitTask  // .SingleOrDefaultAsync(fun x -> x.Id.Equals(deviceId))

        match task with
        | null -> return None
        | d -> return Some(d)
    }

let getDevice (deviceId: Guid) =
    getDeviceAsync deviceId |> Async.RunSynchronously

let updateDeviceById (deviceId: Guid) (device: Devices) =
    None

let webPart = choose [
                rest "devices" {
                    GetAll = getDevices
                    Create = createDevice
                    //Update = Db_Dictionary.updatePerson
                    Delete = deleteDevice
                    GetById = getDevice
                    UpdateById = updateDeviceById
                    //IsExists = Db_Dictionary.isPersonExists
                }
            ]
