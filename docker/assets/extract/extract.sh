#!/usr/bin/with-contenv bash

if [ -z "$(ls -A /localize/lang)" ]; then
    cp -RT /app/www/lang             /localize/lang
fi

if [ -z "$(ls -A /localize/views)" ]; then
    cp -RT /app/www/resources/views  /localize/views
fi
