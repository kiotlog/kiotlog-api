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

namespace Newtonsoft.Json.FSharp

open System
open Microsoft.FSharp.Reflection

open Newtonsoft.Json

/// F# options-converter
type OptionConverter() =
  inherit JsonConverter()

  // let logger = Logging.getLoggerByName "Newtonsoft.Json.FSharp.OptionConverter"

  override _x.CanConvert t =
    t.IsGenericType
    && typedefof<option<_>>.Equals (t.GetGenericTypeDefinition())

  override _x.WriteJson(writer, value, serializer) =
    let value =
      match value with
      | null -> null
      | _ ->
        let _,fields = FSharpValue.GetUnionFields(value, value.GetType())
        fields.[0]
    serializer.Serialize(writer, value)

  override _x.ReadJson(reader, t, _existingValue, serializer) =
    let innerType = t.GetGenericArguments().[0]

    let innerType =
      if innerType.IsValueType then
        typedefof<Nullable<_>>.MakeGenericType([| innerType |])
      else
        innerType

    let value = serializer.Deserialize(reader, innerType)
    let cases = FSharpType.GetUnionCases t

    match value with
    | null -> FSharpValue.MakeUnion(cases.[0], [||])
    | _ -> FSharpValue.MakeUnion(cases.[1], [|value|])
