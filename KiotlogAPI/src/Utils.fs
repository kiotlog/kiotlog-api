module Kiotlog.Web.Utils

open System.Text.RegularExpressions

let (|RegexMatch|_|) pattern input =
    let matches = Regex.Match(input, pattern, RegexOptions.Compiled ||| RegexOptions.IgnoreCase)
    if matches.Success
    then Some matches.Groups
    else None