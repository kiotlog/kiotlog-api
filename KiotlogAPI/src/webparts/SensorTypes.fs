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

let private loadSensorTypeWithSensorsAsync (ctx : KiotlogDBContext) (sensortypeId : Guid) =
    async {
        try
            let sensortypes = ctx.SensorTypes.Include("Sensors")
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

        return! loadSensorTypeWithSensorsAsync ctx sensortypeId
    }

let getSensorType (cs : string) (sensortypeId: Guid) =
    getSensorTypeAsync cs sensortypeId |> Async.RunSynchronously

(*
let updateSensorTypeByIdAsync (cs : string) (sensortypeId: Guid) (sensortype: SensorTypes) =
    async {
        use ctx = getContext cs

        sensortype.Id <- sensortypeId

        let! res = loadSensorTypeWithSensorsAsync ctx sensortypeId

        match res with
        | Error _ -> return res
        | Ok entity ->
            // d |> ctx.SensorTypes.Remove |> ignore
            if not (String.IsNullOrEmpty sensortype.SensorType) then entity.SensorType <- sensortype.SensorType
            if not (isNull sensortype.Auth) then entity.Auth <- sensortype.Auth
            if not (isNull sensortype.Frame) then entity.Frame <- entity.Frame

            if not (isNull sensortype.Sensors) && sensortype.Sensors.Count > 0 then
                let updateSensor = fun (s : Sensors) ->
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
                    if s.ConversionId.HasValue then
                        existing.ConversionId <- s.ConversionId
                    if s.SensorTypeId.HasValue then
                        existing.SensorTypeId <- s.SensorTypeId
                    if not (isNull s.Fmt) then
                        existing.Fmt <- s.Fmt

                sensortype.Sensors |> Seq.iter updateSensor

            try
                ctx.SaveChanges() |> ignore
                return Ok entity
            with
            | :? DbUpdateException -> return Error { Errors = [|"Error updating SensorType"|]; Status = HTTP_409 }
            | _ -> return Error { Errors = [|"Some DB error occurred"|]; Status = HTTP_500 }
    }
let updateSensorTypeById (cs : string) (sensortypeId: Guid) (sensortype: SensorTypes) =
    updateSensorTypeByIdAsync cs sensortypeId sensortype |> Async.RunSynchronously
*)
let updateSensorTypeById (cs : string) (sensortypeId: Guid) (sensortype: SensorTypes) =
    async { return Ok sensortype } |> Async.RunSynchronously

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
            //Update = Db_Dictionary.updatePerson
            Delete = deleteSensorType cs
            GetById = getSensorType cs
            UpdateById = updateSensorTypeById cs
            //IsExists = Db_Dictionary.isPersonExists
        }
    ]
