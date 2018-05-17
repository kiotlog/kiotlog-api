module State exposing (init, update)

import Navigation exposing (Location)
import Routing exposing (extractRoute)
import RemoteData exposing (WebData, fromMaybe)
import Types exposing (Msg(..), Model, Route(..), Page(..), Device, Sensor)
import Rest exposing (fetchDevicesCommand, fetchDeviceCommand, createDeviceCommand, fetchSensorTypesCommand, fetchConversionsCommand)
import Ports exposing (initMDC, openDrawer, closeDrawer)
import Table exposing (initialSort)
import Http exposing (Error(..))


emptyDevice : Device
emptyDevice =
    { id = ""
    , device = ""
    , meta =
        { name = ""
        , description = ""
        }
    , frame =
        { bigendian = False
        , bitfields = False
        }
    , sensors = []
    }


emptySensor : Sensor
emptySensor =
    { sensorTypeId = ""
    , conversionId = ""
    , meta =
        { name = ""
        , description = ""
        }
    , fmt =
        { index = 0
        , fmtChr = ""
        }
    }


initialModel : Model
initialModel =
    { devices = RemoteData.NotAsked
    , devicesTable = (Table.initialSort "Id")
    , device = RemoteData.NotAsked
    , sensorTypes = RemoteData.NotAsked
    , conversions = RemoteData.NotAsked
    , newDevice = emptyDevice
    , currentRoute = NotFoundRoute
    , pageState = BlankPage
    }


init : Location -> ( Model, Cmd Msg )
init location =
    let
        currentRoute =
            Routing.extractRoute location

        ( model, cmd ) =
            setRoute currentRoute initialModel
    in
        model ! [ initMDC (), cmd ]


setRoute : Route -> Model -> ( Model, Cmd Msg )
setRoute route model =
    let
        page pageState =
            ( { model
                | currentRoute = route
                , pageState = pageState
              }
            , closeDrawer ()
            )
    in
        case route of
            DashboardRoute ->
                page DashboardPage

            DevicesRoute ->
                let
                    ( newModel, pageCmd ) =
                        page DevicesPage
                in
                    { newModel | devices = RemoteData.Loading } ! [ pageCmd, fetchDevicesCommand ]

            DeviceRoute id ->
                let
                    dev =
                        case model.devices of
                            RemoteData.Success devices ->
                                findDeviceById id devices |> fromMaybe (BadUrl id)

                            RemoteData.Failure httpError ->
                                RemoteData.Failure httpError

                            RemoteData.NotAsked ->
                                RemoteData.NotAsked

                            RemoteData.Loading ->
                                RemoteData.Loading

                    ( newModel, pageCmd ) =
                        page DevicePage
                in
                    { newModel | device = dev } ! [ pageCmd, (fetchDeviceCommand id) ]

            NewDeviceRoute ->
                let
                    ( newModel, pageCmd ) =
                        page AddDevicePage
                in
                    { newModel | newDevice = emptyDevice } ! [ pageCmd, fetchSensorTypesCommand, fetchConversionsCommand ]

            _ ->
                page NotFoundPage


findDeviceById : String -> List Device -> Maybe Device
findDeviceById devId devices =
    devices
        |> List.filter (\device -> device.id == devId)
        |> List.head


update : Msg -> Model -> ( Model, Cmd Msg )
update msg model =
    case msg of
        NoOp ->
            ( model, Cmd.none )

        OpenDrawer ->
            ( model, openDrawer () )

        CloseDrawer ->
            ( model, closeDrawer () )

        LocationChanged location ->
            setRoute (Routing.extractRoute location) model

        FetchDevices ->
            ( { model | devices = RemoteData.Loading }, fetchDevicesCommand )

        DevicesReceived response ->
            ( { model | devices = response }, Cmd.none )

        SetDevicesTableState newstate ->
            ( { model | devicesTable = newstate }
            , Cmd.none
            )

        DeviceReceived response ->
            ( { model | device = response }, Cmd.none )

        SensorTypesReceived response ->
            ( { model | sensorTypes = response }, Cmd.none )

        ConversionsReceived response ->
            ( { model | conversions = response }, Cmd.none )

        NewDeviceDevice deviceId ->
            let
                updatedNewDevice =
                    setDeviceDevice deviceId model.newDevice
            in
                ( { model | newDevice = updatedNewDevice }, Cmd.none )

        NewDeviceName name ->
            let
                updatedNewDevice =
                    setDeviceName name model.newDevice
            in
                ( { model | newDevice = updatedNewDevice }, Cmd.none )

        NewDeviceBigendian on ->
            let
                updatedNewDevice =
                    setDeviceBigendian on model.newDevice
            in
                ( { model | newDevice = updatedNewDevice }, Cmd.none )

        AddSensor ->
            let
                newDev =
                    model.newDevice

                sensors =
                    newDev.sensors ++ [ emptySensor ]
            in
                ( { model | newDevice = { newDev | sensors = Debug.log "Sensors" sensors } }, Cmd.none )

        RemoveSensorOnDevice idx ->
            let
                newDev =
                    model.newDevice

                sensorsLeft =
                    (List.take (idx) newDev.sensors)
                        ++ (List.drop (idx + 1) newDev.sensors)
            in
                ( { model | newDevice = { newDev | sensors = sensorsLeft } }, Cmd.none )

        SetSensorNameOnDevice idx name ->
            let
                newDevice =
                    updateSensorOnDevice idx (setSensorName name) model.newDevice
            in
                ( { model | newDevice = newDevice }, Cmd.none )

        SetSensorDescrOnDevice idx descr ->
            let
                newDevice =
                    updateSensorOnDevice idx (setSensorDescr descr) model.newDevice
            in
                ( { model | newDevice = newDevice }, Cmd.none )

        SetSensorTypeOnDevice idx id ->
            let
                newDevice =
                    updateSensorOnDevice idx (setSensorType id) model.newDevice
            in
                ( { model | newDevice = newDevice }, Cmd.none )

        SetSensorConversionOnDevice idx id ->
            let
                newDevice =
                    updateSensorOnDevice idx (setConversion id) model.newDevice
            in
                ( { model | newDevice = newDevice }, Cmd.none )

        SetSensorFmtChrOnDevice idx fmtChr ->
            let
                newDevice =
                    updateSensorOnDevice idx (setFmtChr fmtChr) model.newDevice
            in
                ( { model | newDevice = newDevice }, Cmd.none )

        CreateNewDevice ->
            ( model, createDeviceCommand model.newDevice )

        DeviceCreated (Ok device) ->
            -- should add new device in device list?
            ( model, Cmd.none )

        DeviceCreated (Err _) ->
            -- TODO display error
            ( model, Cmd.none )



-- update/add device helpers


updateNewDevice :
    a
    -> (a -> Device -> Device)
    -> Model
    -> ( Model, Cmd Msg )
updateNewDevice newValue updateFunction model =
    let
        updatedNewDevice =
            updateFunction newValue model.newDevice
    in
        ( { model | newDevice = updatedNewDevice }, Cmd.none )


updateSensorOnDevice : Int -> (Sensor -> Sensor) -> Device -> Device
updateSensorOnDevice idx updateFunc device =
    let
        updateSensor i s =
            let
                f =
                    s.fmt
            in
                if i == idx then
                    updateFunc { s | fmt = { f | index = i } }
                else
                    { s | fmt = { f | index = i } }
    in
        { device | sensors = List.indexedMap updateSensor device.sensors }


setSensorName : String -> Sensor -> Sensor
setSensorName name sensor =
    let
        meta =
            sensor.meta
    in
        { sensor | meta = { meta | name = name } }


setSensorDescr : String -> Sensor -> Sensor
setSensorDescr descr sensor =
    let
        meta =
            sensor.meta
    in
        { sensor | meta = { meta | description = descr } }


setSensorType : String -> Sensor -> Sensor
setSensorType typeId sensor =
    { sensor | sensorTypeId = typeId }


setConversion : String -> Sensor -> Sensor
setConversion conversionId sensor =
    { sensor | conversionId = conversionId }


setFmtChr : String -> Sensor -> Sensor
setFmtChr fmtChr sensor =
    let
        fmt =
            sensor.fmt
    in
        { sensor | fmt = { fmt | fmtChr = fmtChr } }


setDeviceDevice : String -> Device -> Device
setDeviceDevice newDevice device =
    { device | device = newDevice }


setDeviceName : String -> Device -> Device
setDeviceName newName device =
    let
        meta =
            device.meta
    in
        { device | meta = { meta | name = newName } }


setDeviceDescription : String -> Device -> Device
setDeviceDescription newDescr device =
    let
        meta =
            device.meta
    in
        { device | meta = { meta | description = newDescr } }


setDeviceBigendian : Bool -> Device -> Device
setDeviceBigendian newBigendian device =
    let
        frame =
            device.frame
    in
        { device | frame = { frame | bigendian = Debug.log "bigendian: " newBigendian } }


setDeviceBitfields : Bool -> Device -> Device
setDeviceBitfields newBitfields device =
    let
        frame =
            device.frame
    in
        { device | frame = { frame | bitfields = newBitfields } }
