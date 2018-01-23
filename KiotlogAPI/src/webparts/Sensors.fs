module Kiotlog.Web.Webparts.Sensors

open System
open Suave

open Kiotlog.Web.Webparts.Generics
open Kiotlog.Web.RestFul
open KiotlogDB

let getSensors (cs : string) =
    getEntities<Sensors> cs

let createSensor (cs : string) (sensor : Sensors) =
    createEntity<Sensors> cs sensor

let getSensor (cs : string) (sensorId: Guid) =
    getEntity<Sensors> cs sensorId

let updateSensorById<'T when 'T : not struct and 'T : null> (cs : string) (sensorId: Guid) (sensor: Sensors) =
    let updateFunc (entity : Sensors) =
             if not (isNull sensor.Meta) then entity.Meta <- sensor.Meta
             if not (isNull sensor.Fmt) then entity.Fmt <- sensor.Fmt
             if sensor.ConversionId.HasValue then entity.ConversionId <- sensor.ConversionId
             if sensor.SensorTypeId.HasValue then entity.SensorTypeId <- sensor.SensorTypeId

    updateEntityById<Sensors> cs sensorId updateFunc

let deleteSensor (cs : string) (sensorId : Guid) =
    deleteEntity<Sensors> cs sensorId

let webPart (cs : string) =
    choose [
        rest {
            Name = "sensors"
            GetAll = getSensors cs
            Create = createSensor cs
            Delete = deleteSensor cs
            GetById = getSensor cs
            UpdateById = updateSensorById cs
        }
    ]