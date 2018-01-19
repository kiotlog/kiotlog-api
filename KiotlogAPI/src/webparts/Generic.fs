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

module Kiotlog.Web.Webparts.Generics

open System
open Suave
open Kiotlog.Web.DB
open Kiotlog.Web.Railway

open KiotlogDB
open Microsoft.EntityFrameworkCore

let getEntitiesAsync<'T when 'T : not struct> (cs : string) () =
    async {
        use ctx = getContext cs
        let set = ctx.Set<'T>()
        try
            let! entity = set.ToArrayAsync() |> Async.AwaitTask

            return Ok entity
        with | _ -> return Error { Errors = [|"Error getting Sensort Types from DB"|]; Status = HTTP_500 }
    }

let getEntities<'T when 'T : not struct> (cs : string) () =
    getEntitiesAsync<'T> cs () |> Async.RunSynchronously

let createEntity<'T when 'T : not struct> (cs : string) (entity: 'T) =
    use ctx = getContext cs
    let set = ctx.Set<'T>()

    entity |> set.Add |> ignore

    try
        ctx.SaveChanges() |> ignore
        Ok entity
    with
    | :? DbUpdateException -> Error { Errors = [|"Error adding" + entity.GetType().Name|]; Status = HTTP_409 }
    | _ -> Error { Errors = [|"Some DB error occurred"|]; Status = HTTP_500 }

let private loadEntityAsync<'T when 'T : not struct and 'T : null> (ctx : KiotlogDBContext) (entityId : Guid) =
    async {
        try
            let entities = ctx.Set<'T>()
            let! entity = entities.FindAsync entityId |> Async.AwaitTask

            match entity with
            | null -> return Error { Errors = [| entity.GetType().Name + " not found"|]; Status = HTTP_404 }
            | d -> return Ok d
        with
        | _ -> return Error { Errors = [|"Some DB error occurred"|]; Status = HTTP_500 }
    }

let getEntityAsync<'T when 'T : not struct and 'T : null> (cs : string) (entityId : Guid) =
    async {
        use ctx = getContext cs

        return! loadEntityAsync<'T> ctx entityId
    }

let getEntity<'T when 'T : not struct and 'T : null> (cs : string) (entityId: Guid) =
    getEntityAsync<'T> cs entityId |> Async.RunSynchronously

let updateEntityByIdAsync<'T when 'T : not struct and 'T : null> (cs : string) (entityId: Guid) updateFunc =
    async {
        use ctx = getContext cs

        // entity.Id <- entityId

        let! res = loadEntityAsync ctx entityId

        match res with
        | Error _ -> return res
        | Ok (entity : 'T) ->
            updateFunc entity
            // let updateFunc resource entity =
            //      if not (String.IsNullOrEmpty resource.Name) then entity.Name <- resource.Name
            //      if not (isNull resource.Meta) then entity.Meta <- resource.Meta
            //      if not (isNull resource.Type) then entity.Type <- resource.Type

            try
                ctx.SaveChanges() |> ignore
                return Ok entity
            with
            | :? DbUpdateException -> return Error { Errors = [|"Error updating" + entity.GetType().Name|]; Status = HTTP_409 }
            | _ -> return Error { Errors = [|"Some DB error occurred"|]; Status = HTTP_500 }
    }
let updateEntityById<'T when 'T : not struct and 'T : null> (cs : string) (entityId: Guid) updateFunc =
    updateEntityByIdAsync<'T> cs entityId updateFunc |> Async.RunSynchronously

let deleteEntityAsync<'T when 'T : not struct and 'T : null> (cs : string) (entityId : Guid) =
    async {
        use ctx = getContext cs
        let set = ctx.Set<'T>()

        let! entity = set.FindAsync(entityId) |> Async.AwaitTask

        match entity with
        | null -> return Error { Errors = [|entity.GetType().Name + " not found"|]; Status = HTTP_404}
        | d ->
            d |> set.Remove |> ignore

            try
                ctx.SaveChanges() |> ignore
                return Ok ()
            with
            | :? DbUpdateException -> return Error { Errors = [|"Error adding" + entity.GetType().Name|]; Status = HTTP_409 }
            | _ -> return Error { Errors = [|"Some DB error occurred"|]; Status = HTTP_500 }
    }

let deleteEntity (cs : string) (entityId : Guid) =
    deleteEntityAsync cs entityId |> Async.RunSynchronously
