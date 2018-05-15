module State exposing (init, update)

import Navigation exposing (Location)
import Routing exposing (extractRoute)
import RemoteData exposing (WebData, fromMaybe)
import Types exposing (Msg(..), Model, Route(..), Page(..), Device, Sensor)
import Rest exposing (fetchDevicesCommand, fetchDeviceCommand, createDeviceCommand)
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
    { meta =
        { name = ""
        , description = ""
        }
    }


initialModel : Model
initialModel =
    { devices = RemoteData.NotAsked
    , devicesTable = (Table.initialSort "Id")
    , device = RemoteData.NotAsked
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
                    { newModel | newDevice = emptyDevice } ! [ pageCmd ]

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

        CreateNewDevice ->
            ( model, createDeviceCommand model.newDevice )

        AddSensor ->
            let
                newDev =
                    model.newDevice
            in
                ( { model | newDevice = { newDev | sensors = emptySensor :: newDev.sensors } }, Cmd.none )

        DeviceCreated (Ok device) ->
            -- should add new device in device list?
            ( model, Cmd.none )

        DeviceCreated (Err _) ->
            -- TODO display error
            ( model, Cmd.none )



-- update/add device helpers


updateNewDevice :
    String
    -> (String -> Device -> Device)
    -> Model
    -> ( Model, Cmd Msg )
updateNewDevice newValue updateFunction model =
    let
        updatedNewDevice =
            updateFunction newValue model.newDevice
    in
        ( { model | newDevice = updatedNewDevice }, Cmd.none )


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
