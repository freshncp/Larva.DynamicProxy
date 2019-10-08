all: test pack
test:
	dotnet test `pwd`/src/DynamicProxyTests/

pack: rebuild
	rm -rf `pwd`/nuget/.DS_Store
	rm -rf `pwd`/nuget/*/.DS_Store
	rm -rf `pwd`/nuget/*/*/.DS_Store
	rm -rf `pwd`/nuget/*/lib/*/*.pdb
	rm -rf `pwd`/nuget/*/lib/*/*.json
	nuget pack -OutputDirectory `pwd`/packages/ `pwd`/nuget/Larva.DynamicProxy/Larva.DynamicProxy.nuspec

rebuild: clean build

clean:
	rm -rf `pwd`/nuget/.DS_Store
	rm -rf `pwd`/nuget/*/.DS_Store
	rm -rf `pwd`/nuget/*/*/.DS_Store
	rm -rf `pwd`/nuget/*/lib/*

build: build-1_6 build-2_0

build-1_6:
	dotnet build -c Release -f 'netstandard1.6' -o `pwd`/nuget/Larva.DynamicProxy/lib/netstandard1.6/ `pwd`/src/Larva.DynamicProxy/

build-2_0:
	dotnet build -c Release -f 'netstandard2.0' -o `pwd`/nuget/Larva.DynamicProxy/lib/netstandard2.0/ `pwd`/src/Larva.DynamicProxy/
