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
        , map NewDeviceRoute (s "devices" </> s "new")
        , map DeviceRoute (s "devices" </> string)
        , map SensorTypesRoute (s "sensors")
        , map SensorTypeRoute (s "sensors" </> string)
        ]
