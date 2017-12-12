module Kiotlog.Web.Railway

open Newtonsoft.Json
open Suave.Http

type RestError = {
    [<JsonIgnore>]
    Status : HttpCode
    Errors : string[]
}