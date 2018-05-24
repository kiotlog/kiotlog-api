module Utils exposing (..)

import Http


createErrorMessage : Http.Error -> String
createErrorMessage httpError =
    case httpError of
        Http.BadUrl message ->
            message

        Http.Timeout ->
            "Server is taking too long to respond. Please try again later."

        Http.NetworkError ->
            "It appears you don't have an Internet connection right now."

        Http.BadStatus response ->
            response.status.message

        Http.BadPayload message response ->
            message



{--
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
--}
