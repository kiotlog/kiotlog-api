module View exposing (view)

import Html exposing (..)
import Html.Attributes exposing (class)
import Types exposing (Model, Msg, Route(..), Page(..))
import Views.Toolbar as Toolbar exposing (view)
import Views.Drawer as Drawer exposing (view)
import Views.Devices as Devices exposing (viewDevices)


mainPage : Model -> Html Msg
mainPage model =
    case model.pageState of
        BlankPage ->
            div [] [ text "Benvenuto!" ]

        DashboardPage ->
            div [] [ text "dashboard" ]

        DevicesPage ->
            Devices.viewDevices model

        DevicePage ->
            Devices.viewDevice model

        SensorsPage ->
            div [] [ text "Sensors" ]

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
