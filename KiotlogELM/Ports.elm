port module Ports exposing (initMDC, openDrawer, closeDrawer)


port sendData : String -> Cmd msg


port initMDC : () -> Cmd msg


port openDrawer : () -> Cmd msg


port closeDrawer : () -> Cmd msg
