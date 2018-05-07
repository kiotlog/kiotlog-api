module Types exposing (..)

import RemoteData exposing (WebData)
import Navigation exposing (Location)
import Table


type alias Device =
    { id : String
    , device : String
    }


type alias Model =
    { devices :
        { data : WebData (List Device)
        , table : Table.State
        }
    , currentRoute : Route
    }


type Msg
    = NoOp
    | LocationChanged Location
    | OpenDrawer
    | CloseDrawer
    | FetchDevices
    | DevicesReceived (WebData (List Device))
    | SetDevicesTableState Table.State


type Route
    = DashboardRoute
    | DevicesRoute
    | DeviceRoute String
    | NotFoundRoute
