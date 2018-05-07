module Views.Devices exposing (..)

import Html exposing (..)
import Html.Attributes exposing (href, class)
import Html.Events exposing (onClick)
import Http
import Types exposing (..)
import RemoteData exposing (WebData)
import Table exposing (Config, stringColumn, intColumn)


-- import Table


view : Model -> Html Msg
view model =
    div []
        [ button [ onClick FetchDevices, class "mdc-button mdc-button--outlined" ]
            [ text "Reload Devices" ]
        , viewDevicesOrError model
        ]


viewDevicesOrError : Model -> Html Msg
viewDevicesOrError model =
    case model.devices.data of
        RemoteData.NotAsked ->
            text ""

        RemoteData.Loading ->
            h3 [] [ text "Loading..." ]

        RemoteData.Success devices ->
            viewDevices devices (model.devices.table)

        RemoteData.Failure httpError ->
            viewError (createErrorMessage httpError)


viewError : String -> Html Msg
viewError errorMessage =
    let
        errorHeading =
            "Couldn't fetch devices at this time."
    in
        div []
            [ h3 [] [ text errorHeading ]
            , text ("Error: " ++ errorMessage)
            ]


viewDevices : List Device -> Table.State -> Html Msg
viewDevices devices tableState =
    div []
        [ h3 [] [ text "Devices" ]
        , Table.view config tableState devices
        ]


config : Table.Config Device Msg
config =
    Table.config
        { toId = .id
        , toMsg = SetDevicesTableState
        , columns =
            [ Table.stringColumn "Id" .id
            , Table.stringColumn "Device" .device
            , detailsColumn
            ]
        }


detailsColumn : Table.Column Device Msg
detailsColumn =
    Table.veryCustomColumn
        { name = ""
        , viewData = showDeviceLink
        , sorter = Table.unsortable
        }


showDeviceLink : Device -> Table.HtmlDetails Msg
showDeviceLink { id } =
    Table.HtmlDetails []
        [ a [ href ("#/devices/" ++ id) ] [ text "show" ]
        ]



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


viewDevice : Device -> Html Msg
viewDevice device =
    tr []
        [ td []
            [ text device.id ]
        , td []
            [ text device.device ]
        , td []
            [ a [ href ("#/devices/" ++ device.id) ] [ text "show" ] ]
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
