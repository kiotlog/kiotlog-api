module Views.Toolbar exposing (view)

import Html exposing (..)
import Html.Attributes exposing (id, class, type_)
import Html.Events exposing (onClick)
import Types exposing (Msg(OpenDrawer))


view : a -> Html Msg
view model =
    header [ class "mdc-top-app-bar mdc-top-app-bar--fixed" ]
        [ div [ class "mdc-top-app-bar__row" ]
            [ section [ class "mdc-top-app-bar__section mdc-top-app-bar__section--align-start" ]
                [ button
                    [ type_ "button"
                    , onClick OpenDrawer
                    , id "kiotlog-actions-button"
                    , class "material-icons mdc-top-app-bar__navigation-icon"
                    ]
                    [ text "menu" ]
                , span [ class "mdc-top-app-bar__title" ]
                    [ text "Kiotlog" ]
                ]
            ]
        ]
