module Utils exposing (..)

import Char


charFromInt : Int -> Char
charFromInt i =
    if i < 10 then
        Char.fromCode <| i + Char.toCode '0'
    else if i < 36 then
        Char.fromCode <| i - 10 + Char.toCode 'A'
    else
        Debug.crash <| toString i


stringFromInt : Int -> String
stringFromInt i =
    String.fromChar (charFromInt i)
