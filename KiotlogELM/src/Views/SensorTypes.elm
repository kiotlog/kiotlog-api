module Views.SensorTypes exposing (viewSensorTypes, viewSensorType)

import Html exposing (..)
import Html.Attributes exposing (class, type_, attribute, href, style)
import Html.Events exposing (onClick)
import Http
import RemoteData exposing (WebData)
import Types exposing (Msg(..), Model, SensorType)
import Table exposing (Config, stringColumn, intColumn, defaultCustomizations, Status(..), HtmlDetails, customConfig, veryCustomColumn)


viewSensorTypes : Model -> Html Msg
viewSensorTypes model =
    case model.sensorTypes of
        RemoteData.NotAsked ->
            text ""

        RemoteData.Loading ->
            div [ class "kiotlog-page text-center" ]
                [ h1 []
                    [ text "Loading..." ]
                ]

        RemoteData.Success devices ->
            devicesTable devices (model.devicesTable)

        RemoteData.Failure httpError ->
            viewError "Couldn't fetch sensor types at this time." (createErrorMessage httpError)


viewError : String -> String -> Html Msg
viewError errorHeading errorMessage =
    div [ class "kiotlog-page text-center" ]
        [ h3 [] [ text errorHeading ]
        , text errorMessage
        ]


createErrorMessage : Http.Error -> String
createErrorMessage httpError =
    case httpError of
        Http.BadUrl message ->
            message

        Http.Timeout ->
            "Server is taking too long to respond. Please try again later."

        Http.NetworkError ->
            "It appears you don't have an Internet connection right now."

        Http.BadStatus response ->
            response.status.message

        Http.BadPayload message response ->
            message


devicesTable : List SensorType -> Table.State -> Html Msg
devicesTable sensorTypes tableState =
    div [ class "kiotlog-page" ]
        [ div [ class "mdc-layout-grid padding-0" ]
            [ div [ class "mdc-layout-grid__inner" ]
                [ h1 [ class "mdc-layout-grid__cell--span-6 margin-0" ]
                    [ text "Sensor Types" ]
                , h1 [ class "text-right mdc-layout-grid__cell--span-6 margin-0" ]
                    [ button
                        [ type_ "button"
                        , onClick FetchSensorTypes
                        , attribute "data-mdc-auto-init" "MDCRipple"
                        , class "mdc-button"
                        ]
                        [ i [ class "material-icons" ]
                            [ text "refresh" ]
                        ]
                    , a
                        [ href "#/devices/new"
                        , attribute "data-mdc-auto-init" "MDCRipple"
                        , class "mdc-button mdc-button--unelevated"
                        ]
                        [ i [ class "material-icons" ]
                            [ text "add" ]
                        ]
                    ]
                ]
            ]
        , Table.view config tableState sensorTypes
        ]


config : Table.Config SensorType Msg
config =
    customConfig
        { toId = .id
        , toMsg = SetDevicesTableState
        , columns =
            [ Table.stringColumn "Name" .name
            , Table.stringColumn "Type" .type_
            , detailsColumn
            ]
        , customizations =
            { defaultCustomizations
                | tableAttrs = [ class "sensor-types-list" ]
                , thead = simpleThead

                -- | rowAttrs = toRowAttrs
                -- , tbodyAttrs = [ class "mdc-layout-grid" ]
            }
        }


detailsColumn : Table.Column SensorType Msg
detailsColumn =
    veryCustomColumn
        { name = ""
        , viewData = showDeviceLink
        , sorter = Table.unsortable
        }


showDeviceLink : SensorType -> HtmlDetails Msg
showDeviceLink { id } =
    HtmlDetails [ class "action-btns" ]
        [ a
            [ href ("#/sensors/" ++ id)
            , class "mdc-button"
            , attribute "data-mdc-auto-init" "MDCRipple"
            ]
            [ text "show" ]
        ]


simpleThead : List ( String, Status, Attribute msg ) -> HtmlDetails msg
simpleThead headers =
    HtmlDetails [] (List.map simpleTheadHelp headers)


simpleTheadHelp : ( String, Status, Attribute msg ) -> Html msg
simpleTheadHelp ( name, status, onClick ) =
    let
        content =
            case status of
                Unsortable ->
                    [ text name ]

                Sortable selected ->
                    [ if selected then
                        darkGrey "arrow_downward"
                      else
                        lightGrey "arrow_downward"
                    , text name
                    ]

                Reversible Nothing ->
                    [ lightGrey "sort"
                    , text name
                    ]

                Reversible (Just isReversed) ->
                    [ darkGrey
                        (if isReversed then
                            "arrow_upward"
                         else
                            "arrow_downward"
                        )
                    , text name
                    ]
    in
        th [ onClick ] content


darkGrey : String -> Html msg
darkGrey symbol =
    i [ style [ ( "color", "#555" ) ], class "material-icons" ] [ text symbol ]


lightGrey : String -> Html msg
lightGrey symbol =
    i [ style [ ( "color", "#ccc" ) ], class "material-icons" ] [ text symbol ]


viewSensorType : Model -> Html Msg
viewSensorType model =
    case model.sensorType of
        RemoteData.NotAsked ->
            text ""

        RemoteData.Loading ->
            h3 [] [ text "Loading..." ]

        RemoteData.Success device ->
            sensorTypeCards device

        RemoteData.Failure httpError ->
            viewError "Couldn't fetch sensor type." (createErrorMessage httpError)


sensorTypeCards : SensorType -> Html Msg
sensorTypeCards sensorType =
    div [ class "kiotlog-page padding-0" ]
        [ div [ class "mdc-layout-grid" ]
            [ div [ class "mdc-layout-grid__inner" ]
                [ h1 [ class "mdc-layout-grid__cell--span-6 margin-0" ]
                    [ text sensorType.name ]
                , h1 [ class "mdc-layout-grid__cell--span-6 margin-0 text-right" ]
                    [ a
                        [ href "#/sensors"
                        , class "mdc-button"
                        , attribute "data-mdc-auto-init" "MDCRipple"
                        ]
                        [ i [ class "material-icons mdc-button__icon" ]
                            [ text "arrow_back" ]
                        , text "Back"
                        ]
                    ]
                ]
            ]
        , div [ class "mdc-layout-grid kiotlog-container-small" ]
            [ div [ class "mdc-layout-grid__inner" ]
                [ div [ class "mdc-card mdc-layout-grid__cell--span-12 padding-gutter" ]
                    [ div [ class "mdc-layout-grid__inner align-center" ]
                        [ span [ class "mdc-layout-grid__cell--span-6" ]
                            [ text "Type:"
                            ]
                        , h3 [ class "mdc-layout-grid__cell--span-6" ]
                            [ text sensorType.type_
                            ]
                        ]
                    , div [ class "mdc-layout-grid__inner align-center" ]
                        [ span [ class "mdc-layout-grid__cell--span-6" ]
                            [ text "Min:" ]
                        , h3 [ class "mdc-layout-grid__cell--span-6" ]
                            [ text (toString sensorType.meta.min) ]
                        ]
                    , div [ class "mdc-layout-grid__inner align-center" ]
                        [ span [ class "mdc-layout-grid__cell--span-6" ]
                            [ text "Max:" ]
                        , h3 [ class "mdc-layout-grid__cell--span-6" ]
                            [ text (toString sensorType.meta.max) ]
                        ]
                    ]
                ]
            ]
        ]
