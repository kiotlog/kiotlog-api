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

module Kiotlog.Web.Utils

open System
open System.Text.RegularExpressions
open Suave

let uuidRegEx = "([0-9A-F]{8}[-]([0-9A-F]{4}[-]){3}[0-9A-F]{12})$"

let (|RegexMatch|_|) pattern input =
    let matches = Regex.Match(input, pattern, RegexOptions.Compiled ||| RegexOptions.IgnoreCase)
    if matches.Success
    then Some matches.Groups
    else None

let regexPatternRouting regex matcher : WebPart =
    fun (ctx : HttpContext) -> async {
        // let resourceRegEx = resourcePath + "/([0-9A-F]{8}[-]([0-9A-F]{4}[-]){3}[0-9A-F]{12})$"
        match ctx.request.url.AbsolutePath with
        | RegexMatch regex groups ->
            return! matcher ctx groups
            // match Guid.TryParse(groups.[1].Value) with
            // | (true, g) -> return! part g ctx
            // | _ -> return! fail
        | _ -> return! fail
    }

let uuidMatcher part (ctx: HttpContext) (groups: GroupCollection) = 
    match Guid.TryParse(groups.[1].Value) with
        | (true, g) -> part g ctx
        | _ -> fail
