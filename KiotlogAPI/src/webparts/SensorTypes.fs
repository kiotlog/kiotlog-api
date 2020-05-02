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

open Kiotlog.Web.Webparts.Generics
open Kiotlog.Web.RestFul

open KiotlogDBF.Models
open Newtonsoft.Json.Linq
open Kiotlog.Web.Railway

let updateSensorTypeById<'T when 'T : not struct> (cs : string) (sensortypeId: Guid) (sensortype: SensorTypes) =
    let updateFunc (entity : SensorTypes) =
        if not (String.IsNullOrEmpty sensortype.Name) then entity.Name <- sensortype.Name
        if not (String.IsNullOrEmpty sensortype.Type) then entity.Type <- entity.Type
        if not (isNull sensortype.Meta) then entity.Meta <- sensortype.Meta

    updateEntityById<SensorTypes> updateFunc cs ["Sensors"] [] sensortypeId

let patchSensorTypeById (cs : string) (annotatioId: Guid) (annotation: JObject) : Result<SensorTypes, RestError> =
    Error { Errors = [|"will be implemented"|]; Status = HTTP_501 }

let webPart (cs : string) =
    choose [
        rest {
            Name = "sensortypes"
            GetAll = getEntities<SensorTypes> cs
            Create = createEntity<SensorTypes> cs
            Delete = deleteEntity<SensorTypes> cs
            GetById =  getEntity<SensorTypes> cs ["Sensors"] []
            UpdateById = updateSensorTypeById cs
            PatchById = patchSensorTypeById cs
        }
    ]
