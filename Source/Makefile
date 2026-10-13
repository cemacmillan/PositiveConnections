.PHONY: build build15 build16 check-dll check-stale deploy deploymm clean verifyapi

rwproj=/Volumes/expand/projects/rw
modname=PositiveConnections
source_dir:=$(CURDIR)
mod_root:=$(abspath $(source_dir)/..)
dll_15:=../1.5/Assemblies/PositiveConnections.dll
dll_16:=../1.6/Assemblies/PositiveConnections.dll

# Release: both game versions. Never copy one DLL to the other folder — build each csproj.
build: build15 build16

build15:
	@echo "=== Building ${modname} (RimWorld 1.5) ==="
	cd "${source_dir}" && dotnet build "${modname}.1.5.csproj" --configuration Release

build16:
	@echo "=== Building ${modname} (RimWorld 1.6) ==="
	cd "${source_dir}" && dotnet build "${modname}".csproj --configuration Release

check-dll:
	@echo "=== Assembly outputs ==="
	@for f in "${dll_15}" "${dll_16}"; do \
		if [ -f "$$f" ]; then ls -la "$$f"; else echo "MISSING: $$f"; exit 1; fi; \
	done

# Fail if any Source/*.cs is newer than either shipped DLL (run make build before deploy).
check-stale: check-dll
	@newest_src=$$(find "${source_dir}" -name '*.cs' -print0 | xargs -0 stat -f '%m %N' 2>/dev/null | sort -rn | head -1 | cut -d' ' -f1); \
	for f in "${dll_15}" "${dll_16}"; do \
		dll_time=$$(stat -f '%m' "$$f"); \
		if [ "$$newest_src" -gt "$$dll_time" ]; then \
			echo "Stale DLL: $$f is older than newest Source/*.cs — run 'make build'"; \
			exit 1; \
		fi; \
	done
	@echo "DLLs are not older than Source/*.cs"

# Copy MindMattersInterface into 1.5 and 1.6 Assemblies (local testing only; not for distribution).
verifyapi: ../../MindMattersInterface/Assemblies/MindMattersInterface.dll
	cp ../../MindMattersInterface/Assemblies/MindMattersInterface.dll ../1.5/Assemblies
	cp ../../MindMattersInterface/Assemblies/MindMattersInterface.dll ../1.6/Assemblies
	@echo "Copied MindMattersInterface DLL to 1.5/Assemblies and 1.6/Assemblies"

# Deploy for release: build both versions, then tar (no MindMattersInterface in archive).
# INCLUDE_MM=1 skips removal (used by deploymm).
deploy: build check-stale
	@if [ -z "$(INCLUDE_MM)" ]; then rm -f ../1.5/Assemblies/MindMattersInterface.dll ../1.6/Assemblies/MindMattersInterface.dll; fi
	cd ${rwproj} ; pwd ; tar cpf - ${modname} | (cd /Users/cem/Library/Application\ Support/Steam/steamapps/common/RimWorld/Mods/ && tar xf -)

deploymm: verifyapi
	$(MAKE) deploy INCLUDE_MM=1

clean:
	rm -rf obj bin
	rm -f "${dll_15}" ../1.5/Assemblies/PositiveConnections.pdb
	rm -f "${dll_16}" ../1.6/Assemblies/PositiveConnections.pdb
