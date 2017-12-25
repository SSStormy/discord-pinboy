#!/bin/bash

instance='pinboy-run'
container='pinboy'

git pull

docker stop $instance
docker rm $instance
docker build -t $container PinBoy

docker run -dit --restart unless-stopped --name $instance $container
