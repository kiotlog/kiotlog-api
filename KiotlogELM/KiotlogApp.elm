module Main exposing (..)

import Html exposing (..)
import Html.Events exposing (..)
import Http
import Json.Decode as Decode


main : Program Never Model Msg
main =
    Html.program
        { init = init "conversions"
        , view = view
        , update = update
        , subscriptions = subscriptions
        }



-- MODEL


type alias Conversion =
    { uuid : String
    , fun : String
    }


type alias Model =
    { endpoint : String
    , entities : List Conversion
    }


init : String -> ( Model, Cmd Msg )
init endpoint =
    ( Model endpoint []
    , getEntity endpoint
    )



-- UPDATE


type Msg
    = Reload
    | NewConversions (Result Http.Error (List Conversion))


update : Msg -> Model -> ( Model, Cmd Msg )
update msg model =
    case msg of
        Reload ->
            ( model, getEntity model.endpoint )

        NewConversions (Ok conversions) ->
            ( Model model.endpoint conversions, Cmd.none )

        NewConversions (Err _) ->
            ( model, Cmd.none )



-- VIEW


toTableRow : Conversion -> Html Msg
toTableRow c =
    tr []
        [ td [] [ text c.uuid ]
        , td [] [ text c.fun ]
        ]


toTable : List Conversion -> Html Msg
toTable cs =
    table
        []
        ([ thead []
            [ th [] [ text "Id" ]
            , th [] [ text "Fun" ]
            ]
         ]
            ++ List.map toTableRow cs
        )


view : Model -> Html Msg
view model =
    div []
        [ h2 [] [ text (String.toUpper model.endpoint) ]
        , button [ onClick Reload ] [ text "Reload" ]
        , br [] []
        , toTable model.entities
        ]



-- SUBSCRIPTIONS


subscriptions : Model -> Sub Msg
subscriptions model =
    Sub.none



-- HTTP


getEntity : String -> Cmd Msg
getEntity endpoint =
    let
        url =
            "http://hetzner-01.trmpln.com/" ++ endpoint
    in
        Http.send NewConversions (getMetadata url)


getMetadata : String -> Http.Request (List Conversion)
getMetadata url =
    Http.get url (Decode.list decodeConversion)


decodeConversion : Decode.Decoder Conversion
decodeConversion =
    Decode.map2 Conversion
        (Decode.field "Id" Decode.string)
        (Decode.field "Fun" Decode.string)
