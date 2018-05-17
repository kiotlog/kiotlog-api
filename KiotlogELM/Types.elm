module Types exposing (..)

import RemoteData exposing (WebData)
import Navigation exposing (Location)
import Table
import Http


type alias Device =
    { id : String
    , device : String
    , meta : Meta
    , frame : Frame
    , sensors : List Sensor
    }


type alias Sensor =
    { sensorTypeId : String
    , conversionId : String
    , meta : Meta
    , fmt : Fmt
    }


type alias SensorType =
    { id : String
    , name : String
    , type_ : String
    , meta : SensorTypeMeta
    }


type alias Conversion =
    { id : String
    , fun : String
    }


type alias Meta =
    { name : String
    , description : String
    }


type alias Frame =
    { bigendian : Bool
    , bitfields : Bool
    }


type alias SensorTypeMeta =
    { min : Int
    , max : Int
    }


type alias Fmt =
    { index : Int
    , fmtChr : String
    }


type alias Model =
    { devices : WebData (List Device)
    , devicesTable : Table.State
    , device : WebData Device
    , sensorTypes : WebData (List SensorType)
    , conversions : WebData (List Conversion)
    , newDevice : Device
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
    | NewDeviceDevice String
    | NewDeviceName String
    | NewDeviceBigendian Bool
    | CreateNewDevice
    | AddSensor
    | SetSensorNameOnDevice Int String
    | SetSensorDescrOnDevice Int String
    | SetSensorTypeOnDevice Int String
    | SetSensorConversionOnDevice Int String
    | SetSensorFmtChrOnDevice Int String
    | DeviceCreated (Result Http.Error Device)
    | SensorTypesReceived (WebData (List SensorType))
    | ConversionsReceived (WebData (List Conversion))


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
