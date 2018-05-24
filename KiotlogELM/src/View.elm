module View exposing (view)

import Html exposing (..)
import Html.Attributes exposing (class, src, style)
import Types exposing (Model, Msg, Route(..), Page(..))
import Views.Toolbar as Toolbar exposing (view)
import Views.Drawer as Drawer exposing (view)
import Views.Dashboard as Dashboard exposing (viewDashboard)
import Views.Devices as Devices exposing (viewDevices, viewDevice, addDevice)
import Views.SensorTypes as SensorTypes exposing (viewSensorTypes, viewSensorType)


mainPage : Model -> Html Msg
mainPage model =
    case model.pageState of
        BlankPage ->
            div [] [ text "Benvenuto!" ]

        DashboardPage ->
            Dashboard.viewDashboard model

        DevicesPage ->
            Devices.viewDevices model

        DevicePage ->
            Devices.viewDevice model

        AddDevicePage ->
            Devices.addDevice model

        SensorTypesPage ->
            SensorTypes.viewSensorTypes model

        SensorTypePage ->
            SensorTypes.viewSensorType model

        NotFoundPage ->
            notFoundView


view : Model -> Html Msg
view model =
    div []
        [ Toolbar.view []
        , div [ class "kiotlog-wrap mdc-top-app-bar--fixed-adjust" ] [ mainPage model ]
        , Drawer.view []
        ]


notFoundView : Html msg
notFoundView =
    h3 [] [ text "Oops! The page you requested was not found!" ]
