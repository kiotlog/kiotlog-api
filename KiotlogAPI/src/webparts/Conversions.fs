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

module Kiotlog.Web.Webparts.Conversions

open System
open Suave

open Kiotlog.Web.Webparts.Generics
open Kiotlog.Web.RestFul

open KiotlogDB

let updateConversionById<'T when 'T : not struct and 'T : null> (cs : string) (conversionId: Guid) (conversion: Conversions) =
    let updateFunc (entity : Conversions) =
         if not (isNull conversion.Fun) then entity.Fun <- conversion.Fun

    updateEntityById<Conversions> cs conversionId updateFunc

let webPart (cs : string) =
    choose [
        rest {
            Name = "conversions"
            GetAll = getEntities<Conversions> cs
            Create = createEntity<Conversions> cs
            Delete = deleteEntity<Conversions> cs
            GetById =  getEntity<Conversions> cs ["Sensors"]
            UpdateById = updateConversionById cs
        }
    ]