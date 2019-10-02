REM build
dotnet build -c Release -f 'netstandard1.6' -o %~dp0nuget/lib/netstandard1.6/ %~dp0src/Larva.DynamicProxy/
dotnet build -c Release -f 'netstandard2.0' -o %~dp0nuget/lib/netstandard2.0/ %~dp0src/Larva.DynamicProxy/

REM test
dotnet test %~dp0src/DynamicProxyTests/

REM pack
nuget pack -OutputDirectory %~dp0packages/ %~dp0nuget/Larva.DynamicProxy.nuspec
