.PHONY: build deploy clean

rwproj=/Volumes/expand/projects/rw
modname=PositiveConnections

build:
	cd ${rwproj}/${modname}/Source ; \
	dotnet build "${modname}".csproj --configuration Release

deploy:
	cd ${rwproj} ;pwd; tar cpf - ${modname} | (cd /Users/cem/Library/Application\ Support/Steam/steamapps/common/RimWorld/RimWorldMac.app/Mods/ && tar xf -)

clean:
	rm -rf obj bin ../Assemblies/PositiveConnections.dll
