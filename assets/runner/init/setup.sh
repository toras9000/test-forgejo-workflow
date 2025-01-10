#!/usr/bin/env bash

cd /data

touch .runner
chown -R 1000:1000 .runner
chmod 775 .runner
chmod g+s .runner

mkdir -p .cache
chown -R 1000:1000 .cache
chmod 775 .cache
chmod g+s .cache

