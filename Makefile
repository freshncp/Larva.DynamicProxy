all: test pack
test:
	dotnet test `pwd`/src/Larva.DynamicProxy.Tests/

pack: build
	mkdir -p `pwd`/packages
	dotnet pack -c Release `pwd`/src/Larva.DynamicProxy/
	mv `pwd`/src/Larva.DynamicProxy/bin/Release/*.nupkg `pwd`/packages/

build:
	dotnet build -c Release `pwd`/src/Larva.DynamicProxy/
	dotnet build -c Release `pwd`/src/Larva.DynamicProxy.Tests/
