module Views.Devices exposing (viewDevices, viewDevice, addDevice)

import Html exposing (..)
import Html.Attributes exposing (id, href, class, type_, style, for, attribute)
import Html.Events exposing (onClick)
import Http
import Types exposing (..)
import RemoteData exposing (WebData)
import Table exposing (Config, stringColumn, intColumn, defaultCustomizations, Status(..), HtmlDetails, customConfig, veryCustomColumn)


viewDevices : Model -> Html Msg
viewDevices model =
    case model.devices of
        RemoteData.NotAsked ->
            text ""

        RemoteData.Loading ->
            h3 [] [ text "Loading..." ]

        RemoteData.Success devices ->
            devicesTable devices (model.devicesTable)

        RemoteData.Failure httpError ->
            viewError "Couldn't fetch devices at this time." (createErrorMessage httpError)


viewDevice : Model -> Html Msg
viewDevice model =
    case model.device of
        RemoteData.NotAsked ->
            text ""

        RemoteData.Loading ->
            h3 [] [ text "Loading..." ]

        RemoteData.Success device ->
            deviceCards device

        RemoteData.Failure httpError ->
            viewError "Couldn't fetch device." (createErrorMessage httpError)


viewError : String -> String -> Html Msg
viewError errorHeading errorMessage =
    div [ class "kiotlog-page text-center" ]
        [ h3 [] [ text errorHeading ]
        , text errorMessage
        ]


devicesTable : List Device -> Table.State -> Html Msg
devicesTable devices tableState =
    div [ class "kiotlog-page" ]
        [ h1 [ class "text-right" ]
            [ text "Devices"
            , button
                [ type_ "button"
                , onClick FetchDevices
                , class "mdc-button"
                ]
                [ i [ class "material-icons" ]
                    [ text "refresh" ]
                ]
            , a
                [ href "#/devices/new"
                , class "mdc-button mdc-button--unelevated"
                ]
                [ i [ class "material-icons" ]
                    [ text "add" ]
                ]
            ]
        , Table.view config tableState devices
        ]


mapSensors : Sensor -> Html Msg
mapSensors sensor =
    div []
        [ text sensor.meta.name ]


deviceCards : Device -> Html Msg
deviceCards device =
    div [ class "kiotlog-page mdc-layout-grid" ]
        [ div [ class "mdc-layout-grid__inner" ]
            [ div [ class "mdc-card padding-20 mdc-layout-grid__cell--span-6" ]
                [ h3 []
                    [ text ("Id " ++ device.id) ]
                , p []
                    [ text ("Name " ++ device.device) ]
                ]
            , div
                [ class "mdc-card padding-20 mdc-layout-grid__cell--span-6" ]
                [ h3 []
                    [ text "Sensors" ]
                , div []
                    (List.map
                        mapSensors
                        device.sensors
                    )
                ]
            ]
        ]


config : Table.Config Device Msg
config =
    customConfig
        { toId = .id
        , toMsg = SetDevicesTableState
        , columns =
            [ Table.stringColumn "Id" .id
            , Table.stringColumn "Device" .device
            , detailsColumn
            ]
        , customizations =
            { defaultCustomizations
                | tableAttrs = [ class "devices-list" ]
                , thead = simpleThead

                -- | rowAttrs = toRowAttrs
                -- , tbodyAttrs = [ class "mdc-layout-grid" ]
            }
        }


detailsColumn : Table.Column Device Msg
detailsColumn =
    veryCustomColumn
        { name = ""
        , viewData = showDeviceLink
        , sorter = Table.unsortable
        }


showDeviceLink : Device -> HtmlDetails Msg
showDeviceLink { id } =
    HtmlDetails []
        [ a [ href ("#/devices/" ++ id), class "mdc-button" ] [ text "show" ]
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



-- toRowAttrs : Device -> List (Attribute Msg)
-- toRowAttrs device =
--     [ class "mdc-layout-grid__inner"
--     ]
-- tHeadAttrs : ( String, Status, Attribute msg ) -> Html msg
-- tHeadAttrs ( name, status, onClick ) =
--     HtmlDetails [] (List.map simpleTheadHelp headers)
-- viewTableHeader : Html Msg
-- viewTableHeader =
--     tr []
--         [ th []
--             [ text "ID" ]
--         , th []
--             [ text "Title" ]
--         , th []
--             []
--         ]


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


addSensor : Html msg
addSensor =
    div [ class "mdc-layout-grid__inner" ]
        [ div
            [ class "mdc-text-field mdc-layout-grid__cell"
            , attribute "data-mdc-auto-init" "MDCTextField"
            ]
            [ input
                [ type_ "text"
                , id "new_sensor_device_id_num"
                , class "mdc-text-field__input"
                ]
                []
            , label
                [ class "mdc-floating-label"
                , for "new_sensor_device_id_num"
                ]
                [ text "Name" ]
            , div [ class "mdc-line-ripple" ] []
            ]
        ]


addDevice : Model -> Html Msg
addDevice model =
    div [ class "kiotlog-page" ]
        [ div [ class "mdc-layout-grid" ]
            [ div [ class "mdc-layout-grid__inner" ]
                [ h1 [ class "mdc-layout-grid__cell--span-6 margin-0" ]
                    [ a
                        [ href "#/devices"
                        , class "mdc-button"
                        ]
                        [ i [ class "material-icons mdc-button__icon" ]
                            [ text "arrow_back" ]
                        , text "Back"
                        ]
                    , button
                        [ type_ "button"
                        , class "mdc-button mdc-button--unelevated"
                        ]
                        [ i [ class "material-icons mdc-button__icon" ]
                            [ text "check" ]
                        , text "Add"
                        ]
                    ]
                , h1 [ class "mdc-layout-grid__cell--span-6 margin-0 text-right" ]
                    [ text "Add new device" ]
                ]
            , div [ class "mdc-layout-grid__inner" ]
                [ div
                    [ class "mdc-text-field mdc-layout-grid__cell"
                    , attribute "data-mdc-auto-init" "MDCTextField"
                    ]
                    [ input
                        [ type_ "text"
                        , id "new_device_device_id"
                        , class "mdc-text-field__input"
                        ]
                        []
                    , label
                        [ class "mdc-floating-label"
                        , for "new_device_device_id"
                        ]
                        [ text "Device Id" ]
                    , div [ class "mdc-line-ripple" ] []
                    ]
                , div
                    [ class "mdc-text-field mdc-layout-grid__cell"
                    , attribute "data-mdc-auto-init" "MDCTextField"
                    ]
                    [ input
                        [ type_ "text"
                        , id "new_device_name"
                        , class "mdc-text-field__input"
                        ]
                        []
                    , label
                        [ class "mdc-floating-label"
                        , for "new_device_name"
                        ]
                        [ text "Name" ]
                    , div [ class "mdc-line-ripple" ] []
                    ]
                ]
            , div [ class "mdc-layout-grid__inner" ]
                [ h2 [ class "mdc-layout-grid__cell--span-12 text-center" ]
                    [ text "Sensors" ]
                ]
            ]
        ]
