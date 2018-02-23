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

[<AutoOpen>]
module Kiotlog.Web.RestFul

// open Newtonsoft.Json
// open Newtonsoft.Json.Converters
open Suave
open System.Text
open Operators
// open Suave.Http
open Successful
// open RequestErrors
open Filters
open System
open System.Collections.Generic
open System.ComponentModel.DataAnnotations

open Utils
open Json
open Railway

type RestResource<'a> = {
    Name : string
    GetAll : unit -> Result<'a [], RestError>
    Create : 'a -> Result<'a, RestError>
    // Update : 'a -> 'a option
    Delete : Guid -> Result<unit, RestError>
    GetById : Guid -> Result<'a, RestError>
    UpdateById : Guid -> 'a -> Result<'a, RestError>
    // IsExists : int -> bool
}

let rest resource =
    let resourcePath = "/" + resource.Name
    // let resourceIdPath = new PrintfFormat<(Guid -> string),unit,string,string,Guid>(resourcePath + "/%s")

    // let badRequest = BAD_REQUEST "Resource not found"

    let validate (entity : 'a) =
        let vctx = ValidationContext(entity)
        let results = new List<ValidationResult>()
        let isValid = Validator.TryValidateObject(entity, vctx, results)
        // (isValid, results)
        match isValid with
        | true -> Ok entity
        | false -> Error { Errors = results |> Seq.map(fun x -> x.ErrorMessage) |> Seq.toArray; Status = HTTP_422 }

    // let handleValidate : WebPart =
    //     fun (ctx : HttpContext) -> async {
    //         return! NOT_FOUND "ciao" ctx
    //     }

    // let handleResource requestError = function
    //     | Some r -> r |> JSON OK
    //     | _ -> requestError

    let handleRailwayResource = function
        | Ok x -> JSON OK x
        | Error e -> JSON (Encoding.UTF8.GetBytes >> Response.response e.Status) e

    let getAll = warbler (fun _ -> resource.GetAll () |> handleRailwayResource)

    let getResourceById =
        resource.GetById >> handleRailwayResource

    let updateResourceById id =
        request (getResourceFromReq >> Result.bind (resource.UpdateById id) >> handleRailwayResource)

    let deleteResourceById id =
        match resource.Delete id with
        | Ok _ -> NO_CONTENT
        | Error e -> JSON (Encoding.UTF8.GetBytes >> Response.response e.Status) e

    //let isResourceExists id =
    //    if resource.IsExists id then OK "" else NOT_FOUND ""

    let uuidPatternRouting part : WebPart =
        fun (ctx : HttpContext) -> async {
            let resourceRegEx = resourcePath + "/([0-9A-F]{8}[-]([0-9A-F]{4}[-]){3}[0-9A-F]{12})$"
            match ctx.request.url.AbsolutePath with
            | RegexMatch resourceRegEx groups ->
                match Guid.TryParse(groups.[1].Value) with
                | (true, g) -> return! part g ctx
                | _ -> return! fail
            | _ -> return! fail
        }

    choose [
        path resourcePath >=> choose [
            GET >=> getAll
            POST >=> request (getResourceFromReq >> Result.bind validate >> Result.bind resource.Create >> handleRailwayResource)
            //PUT >=> request (getResourceFromReq >> resource.Update >> handleResource badRequest)
        ]
        // DELETE >=> pathScan resourceIdPath deleteResourceById
        // GET >=> pathScan resourceIdPath getResourceById
        // PUT >=> pathScan resourceIdPath updateResourceById
        DELETE >=> uuidPatternRouting deleteResourceById
        GET >=> uuidPatternRouting getResourceById
        PUT >=> uuidPatternRouting updateResourceById
        //HEAD >=> pathScan resourceIdPath isResourceExists
    ]
