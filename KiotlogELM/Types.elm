module Types exposing (..)

import RemoteData exposing (WebData)
import Navigation exposing (Location)
import Table


type alias Sensor =
    { meta : Meta
    }


type alias Meta =
    { name : String
    , description : String
    }


type alias Frame =
    { bigendian : Bool
    , bitfields : Bool
    }


type alias Device =
    { id : String
    , device : String
    , meta : Meta
    , frame : Frame
    , sensors : List Sensor
    }


type alias Model =
    { devices : WebData (List Device)
    , devicesTable : Table.State
    , device : WebData Device
    , currentRoute : Route
    , pageState : Page
    }


type Msg
    = NoOp
    | LocationChanged Location
    | OpenDrawer
    | CloseDrawer
    | FetchDevices
    | DevicesReceived (WebData (List Device))
    | SetDevicesTableState Table.State
    | DeviceReceived (WebData Device)


type Route
    = DashboardRoute
    | DevicesRoute
    | NewDeviceRoute
    | DeviceRoute String
    | NotFoundRoute


type Page
    = BlankPage
    | NotFoundPage
    | DashboardPage
    | DevicesPage
    | DevicePage
    | AddDevicePage
    | SensorsPage
