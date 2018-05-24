module Rest
    exposing
        ( fetchKiotlogStatusCommand
        , fetchDevicesCommand
        , fetchDeviceCommand
        , createDeviceCommand
        , fetchSensorTypesCommand
        , fetchSensorTypeCommand
        , fetchConversionsCommand
        , updateSensorCommand
        )

import Types exposing (..)
import Http exposing (..)
import RemoteData exposing (WebData)
import Json.Decode exposing (string, int, list, bool, Decoder, maybe, andThen, fail, succeed)
import Json.Decode.Pipeline exposing (decode, required, optional)
import Json.Encode as Encode
import Date exposing (Date)


apiBaseUrl : String
apiBaseUrl =
    "http://localhost:8888/"


get : String -> Decoder a -> (WebData a -> Msg) -> Cmd Msg
get url decoder msg =
    decoder
        |> Http.get (apiBaseUrl ++ url)
        |> RemoteData.sendRequest
        |> Cmd.map msg


post : a -> String -> (a -> Encode.Value) -> Decoder a -> (Result Error a -> Msg) -> Cmd Msg
post obj endpoint encoder decoder msg =
    let
        request =
            Http.request
                { method = "POST"
                , headers = []
                , url = apiBaseUrl ++ endpoint
                , body = Http.jsonBody (encoder obj)
                , expect = Http.expectJson decoder
                , timeout = Nothing
                , withCredentials = False
                }
    in
        request |> Http.send msg


patch : a -> String -> (a -> Encode.Value) -> Decoder a -> (Result Error a -> Msg) -> Cmd Msg
patch obj endpoint encoder decoder msg =
    let
        request =
            Http.request
                { method = "PATCH"
                , headers = []
                , url = apiBaseUrl ++ endpoint
                , body = Http.jsonBody (encoder obj)
                , expect = Http.expectJson decoder
                , timeout = Nothing
                , withCredentials = False
                }
    in
        request |> Http.send msg


stringToDate : Decoder Date
stringToDate =
    string
        |> andThen
            (\val ->
                case Date.fromString val of
                    Err err ->
                        fail err

                    Ok date ->
                        succeed date
            )


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


fmtDecoder : Decoder Fmt
fmtDecoder =
    decode Fmt
        |> required "Index" int
        |> required "FmtChr" string


sensorDecoder : Decoder Sensor
sensorDecoder =
    decode Sensor
        |> required "Id" string
        |> required "SensorTypeId" string
        |> required "ConversionId" string
        |> required "Meta" metaDecoder
        |> required "Fmt" fmtDecoder
        |> optional "SensorType" (maybe sensorTypeDecoder) Nothing


deviceDecoder : Decoder Device
deviceDecoder =
    decode Device
        |> required "Id" string
        |> optional "Device" string "-- no name --"
        |> required "Meta" metaDecoder
        |> optional "Frame" frameDecoder { bigendian = False, bitfields = False }
        |> optional "Sensors" (list sensorDecoder) []


stMetaDecoder : Decoder SensorTypeMeta
stMetaDecoder =
    decode SensorTypeMeta
        |> required "Min" int
        |> required "Max" int


sensorTypeDecoder : Decoder SensorType
sensorTypeDecoder =
    decode SensorType
        |> required "Id" string
        |> required "Name" string
        |> required "Type" string
        |> required "Meta" stMetaDecoder


conversionsDecoder : Decoder Conversion
conversionsDecoder =
    decode Conversion
        |> required "Id" string
        |> required "Fun" string


statusDeviceDecoder : Decoder StatusDevice
statusDeviceDecoder =
    decode StatusDevice
        |> required "Id" string
        |> required "Device" string
        |> optional "LastPoint" (maybe stringToDate) Nothing


statusSensorTypeGroupDecoder : Decoder StatusSensorTypeGroup
statusSensorTypeGroupDecoder =
    decode StatusSensorTypeGroup
        |> required "Type" string
        |> required "Count" int


statusSensorsDecoder : Decoder StatusSensor
statusSensorsDecoder =
    decode StatusSensor
        |> required "Total" int
        |> required "Types" (list statusSensorTypeGroupDecoder)


kiotlogStatusDecoder : Decoder KiotlogStatus
kiotlogStatusDecoder =
    decode KiotlogStatus
        |> required "Devices" (list statusDeviceDecoder)
        |> required "Sensors" statusSensorsDecoder


metaEncoder : Meta -> Encode.Value
metaEncoder meta =
    Encode.object
        [ ( "Name", Encode.string meta.name )
        , ( "Description", Encode.string meta.description )
        ]


frameEncoder : Frame -> Encode.Value
frameEncoder frame =
    Encode.object
        [ ( "Bigendian", Encode.bool frame.bigendian )
        , ( "Bitfields", Encode.bool frame.bitfields )
        ]


fmtEncoder : Fmt -> Encode.Value
fmtEncoder fmt =
    Encode.object
        [ ( "Index", Encode.int fmt.index )
        , ( "FmtChr", Encode.string fmt.fmtChr )
        ]


sensorEncoder : Sensor -> Encode.Value
sensorEncoder sensor =
    Encode.object
        [ ( "SensorTypeId", Encode.string sensor.sensorTypeId )
        , ( "ConversionId", Encode.string sensor.conversionId )
        , ( "Meta", metaEncoder sensor.meta )
        , ( "Fmt", fmtEncoder sensor.fmt )
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
        [ ( "Device", Encode.string device.device )
        , ( "Meta", metaEncoder device.meta )
        , ( "Frame", frameEncoder device.frame )
        , ( "Sensors", sensorsEncoder device.sensors )
        ]


fetchDevicesCommand : Cmd Msg
fetchDevicesCommand =
    get "devices" (list deviceDecoder) DevicesReceived


fetchDeviceCommand : String -> Cmd Msg
fetchDeviceCommand id =
    get ("devices/" ++ id) deviceDecoder DeviceReceived


fetchSensorTypesCommand : Cmd Msg
fetchSensorTypesCommand =
    get "sensortypes" (list sensorTypeDecoder) SensorTypesReceived


fetchSensorTypeCommand : String -> Cmd Msg
fetchSensorTypeCommand id =
    get ("sensortypes/" ++ id) sensorTypeDecoder SensorTypeReceived


fetchConversionsCommand : Cmd Msg
fetchConversionsCommand =
    get "conversions" (list conversionsDecoder) ConversionsReceived


fetchKiotlogStatusCommand : Cmd Msg
fetchKiotlogStatusCommand =
    get "status" kiotlogStatusDecoder KiotlogStatusReceived


createDeviceCommand : Device -> Cmd Msg
createDeviceCommand device =
    post device "devices" newDeviceEncoder deviceDecoder DeviceCreated


updateSensorCommand : Sensor -> Cmd Msg
updateSensorCommand sensor =
    patch sensor ("sensors/" ++ sensor.id) sensorEncoder sensorDecoder SensorUpdated



-- createDeviceRequest device
--     |> Http.send DeviceCreated
-- createDeviceRequest : Device -> Http.Request Device
-- createDeviceRequest device =
--     Http.request
--         { method = "POST"
--         , headers = []
--         , url = apiBaseUrl ++ "devices"
--         , body = Http.jsonBody (newDeviceEncoder device)
--         , expect = Http.expectJson deviceDecoder
--         , timeout = Nothing
--         , withCredentials = False
--         }
