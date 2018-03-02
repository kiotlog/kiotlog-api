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
open Microsoft.EntityFrameworkCore

open Suave
open Kiotlog.Web.DB
open Kiotlog.Web.Railway

open KiotlogDBF.Context

let private getEntitiesAsync<'T when 'T : not struct and 'T : null> (cs : string) () =
    async {
        use ctx = getContext cs
        let set = ctx.Set<'T>()
        try
            let! entity = set.ToArrayAsync() |> Async.AwaitTask

            return Ok entity
        with | _ -> return Error { Errors = [|"Error getting Sensort Types from DB"|]; Status = HTTP_500 }
    }

let getEntities<'T when 'T : not struct and 'T : null> (cs : string) () =
    getEntitiesAsync<'T> cs () |> Async.RunSynchronously

let createEntity<'T when 'T : not struct> (cs : string) (entity: 'T) =
    use ctx = getContext cs
    let set = ctx.Set<'T>()

    entity |> set.Add |> ignore

    try
        ctx.SaveChanges() |> ignore
        Ok entity
    with
    | :? DbUpdateException -> Error { Errors = [|"Error adding " + typeof<'T>.Name|]; Status = HTTP_409 }
    | _ -> Error { Errors = [|"Some DB error occurred"|]; Status = HTTP_500 }

let private loadEntityAsync<'T when 'T : not struct and 'T : null> (ctx : KiotlogDBFContext) (collections: string list) (references: string list) (entityId : Guid) =
    async {
        try
            let entitiesSet = ctx.Set<'T>()
            let! entity = entitiesSet.FindAsync entityId |> Async.AwaitTask

            match entity with
            | null -> return Error { Errors = [| typeof<'T>.Name + " not found"|]; Status = HTTP_404 }
            | e ->
                match collections with
                | [] -> ()
                | _ -> collections |> List.iter (fun i -> ctx.Entry(e).Collection(i).Load())

                match references with
                | [] -> ()
                | _ -> references |> List.iter (fun i -> ctx.Entry(e).Reference(i).Load())
                return Ok e
        with
        | _ -> return Error { Errors = [|"Some DB error occurred"|]; Status = HTTP_500 }
    }

let private getEntityAsync<'T when 'T : not struct and 'T : null> (cs : string) (collections: string list) (references: string list) (entityId : Guid) =
    async {
        use ctx = getContext cs

        return! loadEntityAsync<'T> ctx collections references entityId
    }

let getEntity<'T when 'T : not struct and 'T: null> (cs : string) (collections: string list) (references: string list) (entityId: Guid) =
    getEntityAsync<'T> cs collections references entityId |> Async.RunSynchronously

let private updateEntityByIdAsync<'T when 'T : not struct and 'T : null> updateFunc (cs : string) (entityId: Guid)  =
    async {
        use ctx = getContext cs

        let! res = loadEntityAsync ctx [] [] entityId

        match res with
        | Error _ -> return res
        | Ok (entity : 'T) ->
            updateFunc entity
            try
                ctx.SaveChanges() |> ignore
                return Ok entity
            with
            | :? DbUpdateException -> return Error { Errors = [| "Error updating " + typeof<'T>.Name |]; Status = HTTP_409 }
            | _ -> return Error { Errors = [|"Some DB error occurred"|]; Status = HTTP_500 }
    }

let updateEntityById<'T when 'T : not struct and 'T : null> updateFunc (cs : string) (entityId: Guid) =
    updateEntityByIdAsync<'T> updateFunc cs entityId  |> Async.RunSynchronously

let private deleteEntityAsync<'T when 'T : not struct> (cs : string) (entityId : Guid) =
    async {
        use ctx = getContext cs
        let set = ctx.Set<'T>()

        let! entity = set.FindAsync entityId |> Async.AwaitTask

        match box entity with
        | null -> return Error { Errors = [| typeof<'T>.Name + " not found"|]; Status = HTTP_404}
        | d ->
            d :?> 'T |> set.Remove |> ignore
            try
                ctx.SaveChanges() |> ignore
                return Ok ()
            with
            | :? DbUpdateException -> return Error { Errors = [|"Error adding " + typeof<'T>.Name|]; Status = HTTP_409 }
            | _ -> return Error { Errors = [|"Some DB error occurred"|]; Status = HTTP_500 }
    }

let deleteEntity<'T when 'T : not struct>  (cs : string) (entityId : Guid) =
    deleteEntityAsync<'T> cs entityId |> Async.RunSynchronously
