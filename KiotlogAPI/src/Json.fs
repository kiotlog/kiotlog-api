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
