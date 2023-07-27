#!/bin/sh
dotnet clean
dotnet build -c Release
mkdir -p ./dist
cp ./kula-cli/bin/Release/net6.0/publish/** ./dist
cp ./assets/kula.cmd ./dist/kula.cmd
