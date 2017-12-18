module Kiotlog.Web.Webparts.Devices

open System
open Suave
open Kiotlog.Web.RestFul
open Kiotlog.Web.DB
open Kiotlog.Web.Railway

open KiotlogDB
open Microsoft.EntityFrameworkCore
open Microsoft.EntityFrameworkCore.Query.Expressions


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

let private loadDeviceWithSensorsAsync (ctx : KiotlogDBContext) (deviceId : Guid) =
    async {
        try
            // let! device = ctx.Devices
            //                 // .FindAsync(deviceId)
            //                 .Where(fun d -> d.Id.Equals(deviceId))                            
            //                 .Include("Sensors.SensorType")
            //                 // .Include(fun d -> d.Sensors)
            //                 // .ThenInclude(fun s -> s.SensorType)
            //                 // .SingleOrDefaultAsync(fun x -> x.Id.Equals(deviceId))
            //                 .SingleOrDefaultAsync()
            //                 |> Async.AwaitTask

            // version of query using C# helpers
            // let! device = Helpers.loadDeviceAsync(ctx, deviceId) |> Async.AwaitTask

            let devices = ctx.Devices.Include("Sensors.SensorType")
            let q =
                query {
                    for d in devices do
                    where (d.Id = deviceId)
                    select d
                }
            let! device = q.SingleOrDefaultAsync () |> Async.AwaitTask

            match device with
            | null -> return Error { Errors = [|"Device not found"|]; Status = HTTP_404 }
            | d -> return Ok d
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
    // use ctx = getContext cs
    // let devices = ctx.Devices
    //                 // .Include(fun d -> d.Sensors)
    //                 // .ThenInclude(fun s -> s.SensorType)
    //                 .Include("Sensors.SensorType")
    // let q = query {
    //             for d in devices do
    //             where (d.Id = deviceId)
    //             select d
    //             exactlyOneOrNone
    //         }
    // match q with
    // | None -> Error { Errors = [|"Device not found"|]; Status = HTTP_404 }
    // | Some d -> Ok d

let updateDeviceByIdAsync (cs : string) (deviceId: Guid) (device: Devices) =
    async {
        use ctx = getContext cs

        device.Id <- deviceId

        // // device |> ctx.Devices.Update |> ignore
        // device |> ctx.Devices.Attach |> ignore

        // // ctx.Entry(device).Property("Device").IsModified <- true

        // let! entity = ctx.Devices.FindAsync(deviceId) |> Async.AwaitTask
        let! res = loadDeviceWithSensorsAsync ctx deviceId

        match res with
        // | null -> return Error { Errors = [|"Device not found"|]; Status = HTTP_404}
        | Error _ -> return res
        | Ok entity ->
            // d |> ctx.Devices.Remove |> ignore
            if not (String.IsNullOrEmpty device.Device) then entity.Device <- device.Device
            if not (isNull device.Auth) then entity.Auth <- device.Auth
            if not (isNull device.Frame) then entity.Frame <- entity.Frame

            // let updateSensors = fun _ v ->
            //     if not (isNull v.Id) then
            //         let sensor = entity.Sensors.FindAsync(v.Id) |> Async.AwaitTask |> Async.RunSynchronously

            //         match sensor with
            //         | null -> ()
            //         | _ -> sensor.Identity

            if not (isNull device.Sensors) && device.Sensors.Count > 0 then
                let updateSensor = fun (s : Sensors) ->
                    // s |> ctx.Sensors.Attach |> ignore
                    // ctx.Entry(s).State <- if s.Id = Guid.Empty then EntityState.Added else EntityState.Modified
                    let existing =
                        let f = 
                            query {
                                for x in entity.Sensors do
                                where (x.Id = s.Id)
                                select x
                                exactlyOneOrDefault
                            } // entity.Sensors.SingleOrDefault(fun x -> x.Id = s.Id)
                        match f with
                        | null ->
                            entity.Sensors.Add s
                            s
                        | _ -> f

                    existing |> ctx.Sensors.Attach |> ignore
                    if not (isNull s.Meta) then
                        existing.Meta <- s.Meta
                        // ctx.Entry(existing).Property("_Meta").IsModified <- true

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
            UpdateById = updateDeviceById cs
            //IsExists = Db_Dictionary.isPersonExists
        }
    ]
