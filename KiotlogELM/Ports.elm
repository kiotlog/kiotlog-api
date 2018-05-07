port module Ports exposing (initMDC)


port sendData : String -> Cmd msg


port initMDC : () -> Cmd msg
