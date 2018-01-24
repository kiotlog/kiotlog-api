module Kiotlog.Web.Webparts.Sensors

open System
open Suave

open Kiotlog.Web.Webparts.Generics
open Kiotlog.Web.RestFul
open KiotlogDB

let updateSensorById<'T when 'T : not struct and 'T : null> (cs : string) (sensorId: Guid) (sensor: Sensors) =
    let updateFunc (entity : Sensors) =
         if not (isNull sensor.Meta) then entity.Meta <- sensor.Meta
         if not (isNull sensor.Fmt) then entity.Fmt <- sensor.Fmt
         if sensor.ConversionId.HasValue then entity.ConversionId <- sensor.ConversionId
         if sensor.SensorTypeId.HasValue then entity.SensorTypeId <- sensor.SensorTypeId

    updateEntityById<Sensors> cs sensorId updateFunc

let webPart (cs : string) =
    choose [
        rest {
            Name = "sensors"
            GetAll = getEntities<Sensors> cs
            Create = createEntity<Sensors> cs
            Delete = deleteEntity<Sensors> cs
            GetById = getEntity<Sensors> cs []
            UpdateById = updateSensorById cs
        }
    ]