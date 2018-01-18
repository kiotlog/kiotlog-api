module internal Kiotlog.Web.Json

open System.Text
open Newtonsoft.Json
open Suave
open Operators
open Railway

let toJsonStr v =
    let jsonSerializerSettings = JsonSerializerSettings()
    // jsonSerializerSettings.ContractResolver <- new CamelCasePropertyNamesContractResolver()
    jsonSerializerSettings.NullValueHandling <- NullValueHandling.Ignore
    jsonSerializerSettings.ReferenceLoopHandling <- ReferenceLoopHandling.Ignore

    // TODO: remove for production
    jsonSerializerSettings.Formatting <- Formatting.Indented

    jsonSerializerSettings.Converters.Add(FSharp.OptionConverter())

    JsonConvert.SerializeObject(v, jsonSerializerSettings)

let JSON webpartCombinator v =
    toJsonStr v
    |> webpartCombinator
    >=> Writers.setMimeType "application/json; charset=utf-8"

let fromJson<'a> json =
    // JsonConvert.DeserializeObject(json, typeof<'a>) :?> 'a
    let jsonSerializerSettings = JsonSerializerSettings()

    jsonSerializerSettings.MissingMemberHandling <- MissingMemberHandling.Error
    // jsonSerializerSettings.Error <- fun sender args ->
    //     ()

    try
        let j = JsonConvert.DeserializeObject<'a>(json, jsonSerializerSettings)
        Ok j
    with
    | e -> Error { Errors = [|"Error parsing JSON body: " + e.Message|]; Status = HTTP_422 }

let getResourceFromReq<'a> (req : HttpRequest) =
    req.rawForm |> Encoding.UTF8.GetString |> fromJson<'a>
