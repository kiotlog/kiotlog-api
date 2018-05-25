FROM node AS build

RUN set -ex \
    && npm update -g \
    && npm install --unsafe-perm -g elm

COPY . /Build
WORKDIR /Build
RUN set -ex \
    && elm-make src/Kiotlog.elm --output=kiotlog.js --yes

FROM nginx:alpine

COPY --from=build Build/kiotlog.js /usr/share/nginx/html
COPY --from=build Build/index.html /usr/share/nginx/html
COPY --from=build Build/css /usr/share/nginx/html/css/
COPY --from=build Build/js /usr/share/nginx/html/js/
COPY --from=build Build/img /usr/share/nginx/html/img/
