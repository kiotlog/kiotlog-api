port module Main exposing (..)

import Types exposing (..)
import View exposing (view)
import Navigation
import State exposing (..)


main : Program Never Model Msg
main =
    Navigation.program LocationChanged
        { init = init
        , view = view
        , update = update
        , subscriptions = \_ -> Sub.none
        }
