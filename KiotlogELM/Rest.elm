module Rest exposing (fetchDevicesCommand)

import Types exposing (..)
import Http exposing (..)
import RemoteData exposing (WebData)
import Json.Decode exposing (string, int, list, Decoder)
import Json.Decode.Pipeline exposing (decode, required, optional)


deviceDecoder : Decoder Device
deviceDecoder =
    decode Device
        |> required "Id" string
        |> optional "Device" string "-- no name --"


fetchDevicesCommand : Cmd Msg
fetchDevicesCommand =
    list deviceDecoder
        |> Http.get "http://localhost:9999/devices"
        |> RemoteData.sendRequest
        |> Cmd.map DevicesReceived
