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

module Kiotlog.Web.Webparts.Sensors

open System
open Suave

open Kiotlog.Web.Webparts.Generics
open Kiotlog.Web.RestFul
open KiotlogDBF.Models

let updateSensorById<'T when 'T : not struct and 'T : null> (cs : string) (sensorId: Guid) (sensor: Sensors) =
    let updateFunc (entity : Sensors) =
         if not (isNull (box sensor.Meta)) then entity.Meta <- sensor.Meta
         if not (isNull (box sensor.Fmt)) then entity.Fmt <- sensor.Fmt
         if Guid.Empty <> sensor.SensorTypeId then entity.SensorTypeId <- sensor.SensorTypeId
         if Guid.Empty <> sensor.ConversionId then entity.ConversionId <- sensor.ConversionId

    updateEntityById<Sensors> updateFunc cs sensorId

let webPart (cs : string) =
    choose [
        rest {
            Name = "sensors"
            GetAll = getEntities<Sensors> cs
            Create = createEntity<Sensors> cs
            Delete = deleteEntity<Sensors> cs
            GetById = getEntity<Sensors> cs [] ["SensorType"; "Conversion"]
            UpdateById = updateSensorById cs
        }
    ]
