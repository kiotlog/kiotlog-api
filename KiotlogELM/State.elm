module State exposing (init, update)

import Navigation exposing (Location)
import Routing exposing (extractRoute)
import RemoteData exposing (WebData)
import Types exposing (Msg(..), Model, Route)
import Rest exposing (fetchDevicesCommand)
import Ports exposing (initMDC, openDrawer, closeDrawer)
import Table exposing (initialSort)


initialModel : Route -> Model
initialModel route =
    { devices = { data = RemoteData.NotAsked, table = (Table.initialSort "Id") }
    , currentRoute = route
    }


init : Location -> ( Model, Cmd msg )
init location =
    let
        currentRoute =
            Routing.extractRoute location
    in
        ( initialModel currentRoute, initMDC () )


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
            ( { model
                | currentRoute = Routing.extractRoute location
              }
            , closeDrawer ()
            )

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
