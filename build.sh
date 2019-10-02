#!/bin/sh
# build
dotnet build -c Release -f 'netstandard1.6' -o `pwd`/nuget/lib/netstandard1.6/ `pwd`/src/Larva.DynamicProxy/
dotnet build -c Release -f 'netstandard2.0' -o `pwd`/nuget/lib/netstandard2.0/ `pwd`/src/Larva.DynamicProxy/

# test
dotnet test `pwd`/src/DynamicProxyTests/

# clean
rm -rf nuget/.DS_Store
rm -rf nuget/*/.DS_Store
rm -rf nuget/*/*/.DS_Store

# pack
nuget pack -OutputDirectory `pwd`/packages/ `pwd`/nuget/Larva.DynamicProxy.nuspec
