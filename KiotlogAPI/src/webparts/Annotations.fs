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

open KiotlogDBF.Models
open Suave.Successful

let updateAnnotationById<'T when 'T : not struct and 'T : null> (cs : string) (conversionId: Guid) (annotation: Annotations) =
    let updateFunc (entity : Annotations) =
         if not (isNull annotation.Description) then entity.Description <- annotation.Description

    updateEntityById<Annotations> updateFunc cs [] [] conversionId

let webPart (cs : string) =
    choose [
        rest {
            Name = "annotations"
            GetAll = fun _ -> Error { Errors = [|"Not available"|]; Status = HTTP_422 } //getEntities<Annotations> cs
            Create = fun _ -> Error { Errors = [|"Not available"|]; Status = HTTP_422 } //createEntity<Annotations> cs
            Delete = deleteEntity<Annotations> cs
            GetById =  getEntity<Annotations> cs [] []
            UpdateById = updateAnnotationById cs
        }
    ]
