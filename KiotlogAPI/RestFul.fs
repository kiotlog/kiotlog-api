[<AutoOpen>]
module Kiotlog.Web.RestFul

open Newtonsoft.Json
// open Newtonsoft.Json.Converters
open Suave
open Operators
// open Suave.Http
open Successful
open RequestErrors
open Filters
open System
open Kiotlog.Web.Utils

type RestResource<'a> = {
    GetAll : unit -> 'a seq
    Create : 'a -> 'a
    // Update : 'a -> 'a option
    Delete : Guid -> unit
    GetById : Guid -> 'a option
    UpdateById : Guid -> 'a -> 'a option
    // IsExists : int -> bool
}

let JSON v =
    let jsonSerializerSettings = JsonSerializerSettings()
    // jsonSerializerSettings.ContractResolver <- new CamelCasePropertyNamesContractResolver()
    jsonSerializerSettings.NullValueHandling <- NullValueHandling.Ignore

    // TODO: remove for production
    jsonSerializerSettings.Formatting <- Formatting.Indented

    // jsonSerializerSettings.Converters.Add(IdiomaticDuConverter())
    jsonSerializerSettings.Converters.Add(Newtonsoft.Json.FSharp.OptionConverter())
    // jsonSerializerSettings.Converters.Add(IsoDateTimeConverter(DateTimeFormat = "yyyy-MM-dd"))

    JsonConvert.SerializeObject(v, jsonSerializerSettings)
    |> OK
    >=> Writers.setMimeType "application/json; charset=utf-8"

let fromJson<'a> json =
    let conv = Newtonsoft.Json.FSharp.OptionConverter()
    let lconv:JsonConverter [] = [|conv|]
    JsonConvert.DeserializeObject(json, typeof<'a>, lconv) :?> 'a

let getResourceFromReq<'a> (req : HttpRequest) = 
    let getString rawForm = System.Text.Encoding.UTF8.GetString(rawForm)
    req.rawForm |> getString |> fromJson<'a>

let rest resourceName resource =
    let resourcePath = "/" + resourceName
    // let resourceIdPath = new PrintfFormat<(Guid -> string),unit,string,string,Guid>(resourcePath + "/%s")

    let badRequest = BAD_REQUEST "Resource not found"

    let handleResource requestError = function
        | Some r -> r |> JSON
        | _ -> requestError

    let getAll = warbler (fun _ -> resource.GetAll () |> JSON)
        
    let getResourceById = 
        resource.GetById >> handleResource (NOT_FOUND "Resource not found")

    let updateResourceById id =
        request (getResourceFromReq >> (resource.UpdateById id) >> handleResource badRequest)

    let deleteResourceById id =
        resource.Delete id
        NO_CONTENT

    //let isResourceExists id =
    //    if resource.IsExists id then OK "" else NOT_FOUND ""

    let uuidPatternRouting part : WebPart =
        let f (r:HttpContext) =
            let resourceRegEx = resourcePath + "/([0-9A-F]{8}[-]([0-9A-F]{4}[-]){3}[0-9A-F]{12})$"
            match r.request.url.AbsolutePath with
            | RegexMatch resourceRegEx groups ->
                match Guid.TryParse(groups.[1].Value) with
                | (true, g) -> part g r
                | _ -> fail
            | _ -> fail
        f

    choose [
        path resourcePath >=> choose [
            GET >=> getAll
            POST >=> request (getResourceFromReq >> resource.Create >> JSON)
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