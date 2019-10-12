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

build: build-1_6 build-2_0 build-net45 build-net46 build-net47

build-1_6:
	dotnet build -c Release -f 'netstandard1.6' -o `pwd`/nuget/Larva.DynamicProxy/lib/netstandard1.6/ `pwd`/src/Larva.DynamicProxy/

build-2_0:
	dotnet build -c Release -f 'netstandard2.0' -o `pwd`/nuget/Larva.DynamicProxy/lib/netstandard2.0/ `pwd`/src/Larva.DynamicProxy/

build-net45:
	msbuild `pwd`/src/Larva.DynamicProxy/Larva.DynamicProxy.csproj -r -t:Rebuild -p:Configuration=Release -p:TargetFramework=net45 -p:OutputPath=`pwd`/nuget/Larva.DynamicProxy/lib/net45/ 

build-net46:
	msbuild `pwd`/src/Larva.DynamicProxy/Larva.DynamicProxy.csproj -r -noConLog -t:Rebuild -p:Configuration=Release -p:TargetFramework=net46 -p:OutputPath=`pwd`/nuget/Larva.DynamicProxy/lib/net46/ 

build-net47:
	msbuild `pwd`/src/Larva.DynamicProxy/Larva.DynamicProxy.csproj -r -noConLog -t:Rebuild -p:Configuration=Release -p:TargetFramework=net47 -p:OutputPath=`pwd`/nuget/Larva.DynamicProxy/lib/net47/ 
