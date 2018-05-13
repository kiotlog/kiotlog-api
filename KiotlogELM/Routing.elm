module Routing exposing (..)

import Navigation exposing (Location)
import UrlParser exposing (..)
import Types exposing (..)


extractRoute : Location -> Route
extractRoute location =
    case (parseHash matchRoute location) of
        Just route ->
            route

        Nothing ->
            NotFoundRoute


matchRoute : Parser (Route -> a) a
matchRoute =
    oneOf
        [ map DashboardRoute top
        , map DashboardRoute (s "dashboard")
        , map DevicesRoute (s "devices")
        , map DeviceRoute (s "devices" </> string)
        ]
