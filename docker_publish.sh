#!/bin/bash
docker build -t spike_core .
docker tag spike_core freenodecsharp/spike_core:latest

echo "$DOCKER_PASSWORD" | docker login -u "$DOCKER_USERNAME" --password-stdin
docker push freenodecsharp/spike_core:latest