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

module Kiotlog.Web.Webparts.Annotations

open System
open Suave

open Kiotlog.Web.Webparts.Generics
open Kiotlog.Web.RestFul
open Kiotlog.Web.Railway
open Kiotlog.Web.Json

open KiotlogDBF.Models
open Newtonsoft.Json.Linq

let updateAnnotationById<'T when 'T : not struct and 'T : null> (cs : string) (annotatioId: Guid) (annotation: Annotations) =
    let updateFunc (entity : Annotations) =
    // this function updates entire object. Updates on other objects shoud follow it.
    // for partial update use PATCH
        entity.Description <- annotation.Description
        entity.Begin <- annotation.Begin
        entity.End <- annotation.End
        entity.Data <- annotation.Data

    updateEntityById<Annotations> updateFunc cs [] [] annotatioId

let patchAnnotationById (cs : string) (annotatioId: Guid) (annotation: JObject) : Result<Annotations, RestError> =
    let updateFunc (entity : Annotations) =
    // this function updates entire object. Updates on other objects shoud follow it.
    // for partial update use PATCH
        if (annotation.ContainsKey "Description") then entity.Description <- getValue annotation "Description"
        if (annotation.ContainsKey "Begin") then entity.Begin <- getValue annotation "Begin"
        if (annotation.ContainsKey "End") then entity.End <- getValue annotation "End"
        if (annotation.ContainsKey "Data") then entity.Data <- getValue annotation "Data"

    Error { Errors = [|"will be implemented"|]; Status = HTTP_501 }

let webPart (cs : string) =
    choose [
        rest {
            Name = "annotations"
            GetAll = fun _ -> Error { Errors = [|"Not available: Get device's annotations with GET /devices/<device_id>/annotations"|]; Status = HTTP_422 } //getEntities<Annotations> cs
            Create = fun _ -> Error { Errors = [|"Not available: Create new annotations for device with POST /devices/<device_id>/annotations"|]; Status = HTTP_422 } //createEntity<Annotations> cs
            Delete = deleteEntity<Annotations> cs
            GetById =  getEntity<Annotations> cs [] []
            UpdateById = updateAnnotationById cs
            PatchById = patchAnnotationById cs
        }
    ]
