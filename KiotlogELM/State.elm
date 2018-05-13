module State exposing (init, update)

import Navigation exposing (Location)
import Routing exposing (extractRoute)
import RemoteData exposing (WebData, fromMaybe)
import Types exposing (Msg(..), Model, Route(..), Page(..), Device)
import Rest exposing (fetchDevicesCommand, fetchDeviceCommand)
import Ports exposing (initMDC, openDrawer, closeDrawer)
import Table exposing (initialSort)
import Http exposing (Error(..))


initialModel : Model
initialModel =
    { devices =
        { data = RemoteData.NotAsked
        , table = (Table.initialSort "Id")
        }
    , device = RemoteData.NotAsked
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
                    newModel ! [ pageCmd, fetchDevicesCommand ]

            DeviceRoute id ->
                let
                    dev =
                        case model.devices.data of
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
            let
                devices =
                    model.devices
            in
                ( { model | devices = { devices | data = RemoteData.Loading } }, fetchDevicesCommand )

        DevicesReceived response ->
            let
                devices =
                    model.devices
            in
                ( { model | devices = { devices | data = response } }, Cmd.none )

        SetDevicesTableState newstate ->
            let
                devices =
                    model.devices
            in
                ( { model | devices = { devices | table = newstate } }
                , Cmd.none
                )

        DeviceReceived response ->
            ( { model | device = response }, Cmd.none )
