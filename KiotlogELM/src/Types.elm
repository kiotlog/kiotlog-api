module Types exposing (..)

import RemoteData exposing (WebData)
import Navigation exposing (Location)
import Table
import Http
import Date exposing (Date)


type alias Device =
    { id : String
    , device : String
    , meta : Meta
    , frame : Frame
    , sensors : List Sensor
    }


type alias Sensor =
    { id : String
    , sensorTypeId : String
    , conversionId : String
    , meta : Meta
    , fmt : Fmt
    , sensorType : Maybe SensorType
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


type alias StatusDevice =
    { id : String
    , device : String
    , lastPoint : Maybe Date
    }


type alias StatusSensorTypeGroup =
    { type_ : String
    , count : Int
    }


type alias StatusSensor =
    { total : Int
    , types : List StatusSensorTypeGroup
    }


type alias KiotlogStatus =
    { devices : List StatusDevice
    , sensors : StatusSensor
    }


type alias Model =
    { devices : WebData (List Device)
    , devicesTable : Table.State
    , status : WebData KiotlogStatus
    , device : WebData Device
    , sensor : WebData Sensor
    , sensorTypes : WebData (List SensorType)
    , sensorType : WebData SensorType
    , conversions : WebData (List Conversion)
    , currentRoute : Route
    , pageState : Page
    , editingId : Maybe String
    }


type Msg
    = NoOp
    | LocationChanged Location
    | OpenDrawer
    | CloseDrawer
    | FetchKiotlogStatus
    | KiotlogStatusReceived (WebData KiotlogStatus)
    | FetchDevices
    | DevicesReceived (WebData (List Device))
    | SetDevicesTableState Table.State
    | DeviceReceived (WebData Device)
    | NewDeviceDevice String
    | NewDeviceName String
    | NewDeviceBigendian Bool
    | CreateNewDevice
    | AddSensor
    | RemoveSensorOnDevice Int
    | SetSensorNameOnDevice Int String
    | SetSensorDescrOnDevice Int String
    | SetSensorTypeOnDevice Int String
    | SetSensorConversionOnDevice Int String
    | SetSensorFmtChrOnDevice Int String
    | DeviceCreated (Result Http.Error Device)
    | FetchSensorTypes
    | SensorTypesReceived (WebData (List SensorType))
    | SensorTypeReceived (WebData SensorType)
    | ConversionsReceived (WebData (List Conversion))
    | StartEditing String
    | CancelEditing
    | EditSensor Sensor
    | PutSensor Sensor
    | SensorUpdated (Result Http.Error Sensor)


type Route
    = DashboardRoute
    | DevicesRoute
    | NewDeviceRoute
    | DeviceRoute String
    | SensorTypesRoute
    | SensorTypeRoute String
    | NotFoundRoute


type Page
    = BlankPage
    | NotFoundPage
    | DashboardPage
    | DevicesPage
    | DevicePage
    | AddDevicePage
    | SensorTypesPage
    | SensorTypePage
