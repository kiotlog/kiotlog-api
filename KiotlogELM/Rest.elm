module Rest exposing (fetchDevicesCommand, fetchDeviceCommand)

import Types exposing (..)
import Http exposing (..)
import RemoteData exposing (WebData)
import Json.Decode exposing (string, int, list, bool, Decoder)
import Json.Decode.Pipeline exposing (decode, required, optional)


apiBaseUrl =
    "http://localhost:9999/"


metaDecoder : Decoder Meta
metaDecoder =
    decode Meta
        |> required "Name" string
        |> optional "Description" string ""


frameDecoder : Decoder Frame
frameDecoder =
    decode Frame
        |> required "Bigendian" bool
        |> required "Bitfields" bool


sensorDecoder : Decoder Sensor
sensorDecoder =
    decode Sensor
        |> required "Meta" metaDecoder


deviceDecoder : Decoder Device
deviceDecoder =
    decode Device
        |> required "Id" string
        |> optional "Device" string "-- no name --"
        |> required "Meta" metaDecoder
        |> optional "Frame" frameDecoder { bigendian = False, bitfields = False }
        |> optional "Sensors" (list sensorDecoder) []


fetchDevicesCommand : Cmd Msg
fetchDevicesCommand =
    list deviceDecoder
        |> Http.get (apiBaseUrl ++ "devices")
        |> RemoteData.sendRequest
        |> Cmd.map DevicesReceived


fetchDeviceCommand : String -> Cmd Msg
fetchDeviceCommand id =
    deviceDecoder
        |> Http.get (apiBaseUrl ++ "devices/" ++ id)
        |> RemoteData.sendRequest
        |> Cmd.map DeviceReceived
