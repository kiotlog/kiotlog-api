# Running Kiotlog Web UI

## Development setup

1. Install [Elm](https://guide.elm-lang.org/install.html) and [elm-live](https://git.io/elm-live).
2. Run Kiotlog platform
3. Run Elm app with `elm-live`

    $ elm-live Kiotlog.elm --output=kiotlog.js --pushstate

## Run in Docker

    $ docker build --rm -f Dockerfile -t kiotlog/webui .
    $ docker run --rm -ti -p 8000:80 --network <kiotlog platform network> kiotlog/webui
