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
    | :? DbUpdateException -> Error { Errors = [|"Error adding Entity"|]; Status = HTTP_409 }
    | _ -> Error { Errors = [|"Some DB error occurred"|]; Status = HTTP_500 }

let private loadEntityAsync<'T when 'T : not struct and 'T : null> (ctx : KiotlogDBContext) (entityId : Guid) =
    async {
        try
            let entities = ctx.Set<'T>()
            let q =
                query {
                    for st in entities do
                    where (st.Id = entityId)
                    select st
                }
            let! entity = q.SingleOrDefaultAsync () |> Async.AwaitTask

            match entity with
            | null -> return Error { Errors = [|"Sensor Type not found"|]; Status = HTTP_404 }
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

// let updateEntityByIdAsync<'T when 'T : not struct and 'T : (member Id : Guid)> (cs : string) (entityId: Guid) (entity: 'T) =
let updateEntityByIdAsync<'T when 'T : not struct> (cs : string) (entityId: Guid) (entity: 'T) =
    async {
        use ctx = getContext cs

        entity.Id <- entityId

        let! res = loadEntityAsync ctx entityId

        match res with
        | Error _ -> return res
        | Ok e ->
            if not (String.IsNullOrEmpty entity.Name) then e.Name <- entity.Name
            if not (isNull entity.Meta) then e.Meta <- entity.Meta
            if not (isNull entity.Type) then e.Type <- entity.Type

            try
                ctx.SaveChanges() |> ignore
                return Ok e
            with
            | :? DbUpdateException -> return Error { Errors = [|"Error updating Entity"|]; Status = HTTP_409 }
            | _ -> return Error { Errors = [|"Some DB error occurred"|]; Status = HTTP_500 }
    }
let updateEntityById<'T when 'T : not struct> (cs : string) (entityId: Guid) (entity: 'T) =
    updateEntityByIdAsync<'T> cs entityId entity |> Async.RunSynchronously

let deleteEntityAsync<'T when 'T : not struct and 'T : null> (cs : string) (entityId : Guid) =
    async {
        use ctx = getContext cs
        let set = ctx.Set<'T>()

        let! entity = set.FindAsync(entityId) |> Async.AwaitTask

        match entity with
        | null -> return Error { Errors = [|"Entity not found"|]; Status = HTTP_404}
        | d ->
            d |> set.Remove |> ignore

            try
                ctx.SaveChanges() |> ignore
                return Ok ()
            with
            | :? DbUpdateException -> return Error { Errors = [|"Error adding Entity"|]; Status = HTTP_409 }
            | _ -> return Error { Errors = [|"Some DB error occurred"|]; Status = HTTP_500 }
    }

let deleteEntity (cs : string) (entityId : Guid) =
    deleteEntityAsync cs entityId |> Async.RunSynchronously
