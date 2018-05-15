module Rest exposing (fetchDevicesCommand, fetchDeviceCommand, createDeviceCommand)

import Types exposing (..)
import Http exposing (..)
import RemoteData exposing (WebData)
import Json.Decode exposing (string, int, list, bool, Decoder)
import Json.Decode.Pipeline exposing (decode, required, optional)
import Json.Encode as Encode


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


createDeviceCommand : Device -> Cmd Msg
createDeviceCommand device =
    createDeviceRequest device
        |> Http.send DeviceCreated


createDeviceRequest : Device -> Http.Request Device
createDeviceRequest device =
    Http.request
        { method = "POST"
        , headers = []
        , url = apiBaseUrl ++ "devices"
        , body = Http.jsonBody (newDeviceEncoder device)
        , expect = Http.expectJson deviceDecoder
        , timeout = Nothing
        , withCredentials = False
        }


metaEncoder : Meta -> Encode.Value
metaEncoder meta =
    Encode.object
        [ ( "name", Encode.string meta.name )
        , ( "description", Encode.string meta.description )
        ]


frameEncoder : Frame -> Encode.Value
frameEncoder frame =
    Encode.object
        [ ( "bigendian", Encode.bool frame.bigendian )
        , ( "bitfields", Encode.bool frame.bitfields )
        ]


sensorEncoder : Sensor -> Encode.Value
sensorEncoder sensor =
    Encode.object
        [ ( "meta", metaEncoder sensor.meta )
        ]


sensorsEncoder : List Sensor -> Encode.Value
sensorsEncoder sensors =
    Encode.list
        (List.map
            sensorEncoder
            sensors
        )


newDeviceEncoder : Device -> Encode.Value
newDeviceEncoder device =
    Encode.object
        [ ( "device", Encode.string device.device )
        , ( "meta", metaEncoder device.meta )
        , ( "frame", frameEncoder device.frame )
        , ( "sensors", sensorsEncoder device.sensors )
        ]
