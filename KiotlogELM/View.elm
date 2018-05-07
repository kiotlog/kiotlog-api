module View exposing (view)

import Html exposing (..)
import Html.Attributes exposing (class)
import Types exposing (Model, Msg, Route(..))
import Toolbar exposing (view)
import Drawer exposing (view)
import Views.Devices as Devices exposing (view)


mainPage : Model -> Html Msg
mainPage model =
    case model.currentRoute of
        DashboardRoute ->
            div [] [ text "dashboard" ]

        DevicesRoute ->
            Devices.view model

        DeviceRoute id ->
            div [] [ text ("device " ++ id) ]

        NotFoundRoute ->
            notFoundView


view : Model -> Html Msg
view model =
    div []
        [ Toolbar.view []
        , div [ class "mdc-top-app-bar--fixed-adjust" ] [ mainPage model ]
        , Drawer.view []
        ]


notFoundView : Html msg
notFoundView =
    h3 [] [ text "Oops! The page you requested was not found!" ]
