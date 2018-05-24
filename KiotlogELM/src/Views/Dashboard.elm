module Views.Dashboard exposing (viewDashboard)

import Html exposing (..)
import Html.Attributes exposing (id, class, src, style)
import Types exposing (..)
import RemoteData exposing (WebData)
import Utils exposing (createErrorMessage)


viewDashboard : Model -> Html Msg
viewDashboard model =
    div [ class "kiotlog-page" ]
        [ div
            [ style
                [ ( "position", "fixed" )
                , ( "top", "0" )
                , ( "bottom", "0" )
                , ( "left", "0" )
                , ( "right", "0" )
                , ( "display", "flex" )
                , ( "align-content", "center" )
                , ( "justify-conten", "center" )
                , ( "justify-content", "center" )
                , ( "z-index", "-1" )
                ]
            ]
            [ img
                [ src "/img/transparent.svg"
                , style [ ( "width", "640px" ) ]
                ]
                []
            ]
        , div [ class "mdc-layout-grid padding-0" ]
            [ div [ class "mdc-layout-grid__inner margin-bottom-gutter" ]
                [ h1 [ class "mdc-layout-grid__cell--span-12 margin-0" ]
                    [ text "Kiotlog" ]
                ]
            , div [ class "mdc-layout-grid__inner margin-bottom-gutter" ]
                [ viewStatusCard model.status viewDevicesStatus
                , viewStatusCard model.status viewSensorsStatus
                ]
            ]
        ]


viewStatusCard : WebData KiotlogStatus -> (KiotlogStatus -> Html Msg) -> Html Msg
viewStatusCard status view =
    case status of
        RemoteData.NotAsked ->
            text ""

        RemoteData.Loading ->
            div [ class "hidden kiotlog-page text-center" ]
                [ h1 []
                    [ text "Loading..." ]
                ]

        RemoteData.Success status ->
            view status

        RemoteData.Failure httpError ->
            div [ class "text-center hidden" ]
                [ h3 [] [ text "Couldn't fetch data at this time." ]
                , text (createErrorMessage httpError)
                ]


viewDevicesStatus : KiotlogStatus -> Html Msg
viewDevicesStatus status =
    div
        [ class "mdc-card mdc-layout-grid__cell--span-6-desktop mdc-layout-grid__cell--span-4-tablet mdc-layout-grid__cell--span-4-phone padding-gutter" ]
        [ div [ class "mdc-layout-grid__inner" ]
            [ h1 [ class "mdc-layout-grid__cell--span-12 text-center" ]
                [ text ("Total Devices: " ++ (toString (List.length status.devices))) ]
            , div [ class "mdc-layout-grid__cell--span-12" ]
                [ div [ class "mdc-layout-grid__inner" ]
                    [ h3 [ class "mdc-layout-grid__cell--span-12 text-center margin-0" ]
                        [ text ("Active: " ++ (toString (List.length (filterActiveDevices status.devices)))) ]
                    , h3 [ class "mdc-layout-grid__cell--span-12 text-center margin-0" ]
                        [ text ("Dead: " ++ (toString (List.length (filterDeadDevices status.devices)))) ]
                    ]
                ]
            ]
        ]


viewSensorsStatus : KiotlogStatus -> Html Msg
viewSensorsStatus status =
    div [ class "mdc-card mdc-layout-grid__cell--span-6-desktop mdc-layout-grid__cell--span-4-tablet mdc-layout-grid__cell--span-4-phone padding-gutter" ]
        [ div [ class "mdc-layout-grid__inner" ]
            ([ h1 [ class "mdc-layout-grid__cell--span-12 text-center margin-0 margin-bottom-gutter" ]
                [ text ("Total Sensors: " ++ (toString status.sensors.total)) ]
             ]
                ++ (showSensorsByType status.sensors.types)
            )
        ]


deviceIsActive : StatusDevice -> Bool
deviceIsActive device =
    case device.lastPoint of
        Nothing ->
            False

        Just d ->
            True


deviceIsDead : StatusDevice -> Bool
deviceIsDead device =
    not (deviceIsActive device)


filterActiveDevices : List StatusDevice -> List StatusDevice
filterActiveDevices devices =
    List.filter deviceIsActive devices


filterDeadDevices : List StatusDevice -> List StatusDevice
filterDeadDevices devices =
    List.filter (deviceIsDead) devices


showSensorType : StatusSensorTypeGroup -> Html Msg
showSensorType sensorType =
    div [ class "mdc-layout-grid__cell--span-12" ]
        [ div [ class "mdc-layout-grid__inner" ]
            [ h3 [ class "margin-0 mdc-layout-grid__cell--span-6-desktop mdc-layout-grid__cell--span-4-tablet mdc-layout-grid__cell--span-2-phone text-right" ]
                [ text sensorType.type_ ]
            , h3 [ class "margin-0 mdc-layout-grid__cell--span-6-desktop mdc-layout-grid__cell--span-4-tablet mdc-layout-grid__cell--span-2-phone" ]
                [ text (toString sensorType.count) ]
            ]
        ]


showSensorsByType : List StatusSensorTypeGroup -> List (Html Msg)
showSensorsByType sensorGroups =
    List.map showSensorType sensorGroups
