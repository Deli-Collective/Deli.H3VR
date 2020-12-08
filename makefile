export SHELL            = /bin/bash

# Settings
export CONFIG          ?= Release
export FRAMEWORK       ?= net35
export VERSION         ?= $(shell git describe --tags --abbrev=0 | sed -n 's/v\([0-9\.]+\)*/\1/p')
       NUGET           ?= nuget
export NUGET_DIR        = ../$(NUGET)

# DLL metadata
export GIT_DESCRIBE     = $(shell git describe --long --always --dirty)
export GIT_BRANCH       = $(shell git rev-parse --abbrev-ref HEAD)
export GIT_HASH         = $(shell git rev-parse HEAD)
export BUILD_PROPERTIES = /p:Version="$(VERSION)" /p:RepositoryBranch="$(GIT_BRANCH)" /p:RepositoryCommit="$(GIT_HASH)"
export BUILD            = dotnet build --configuration $(CONFIG) --framework $(FRAMEWORK) $(BUILD_PROPERTIES)

# Packages
export MANIFEST         = manifest.json
export MANIFEST_OLD     = $(MANIFEST).old
export CONTENTS         = $(MANIFEST)

# Local
NAME                 = Deli.H3VR
ZIP                  = $(NAME).zip
TEMP                 = temp

PROJ_LEGACY          = $(NAME).Legacy
PROJ_LSIIC           = $(PROJ_LEGACY).LSIIC
PROJS                = $(PROJ_LSIIC)

TEMP_MODS            = temp/mods
TEMP_LEGACY          = $(TEMP_MODS)/legacy
TEMP_VIRTUAL_OBJECTS = $(TEMP_LEGACY)/VirtualObjects
TEMP_DIRS            = $(TEMP_MODS) $(TEMP_VIRTUAL_OBJECTS)

CONTENTS_MODS  = $(addsuffix /*.deli,$(PROJS))

.PHONY: FORCE all clean

all: clean $(ZIP)
FORCE:

$(PROJS): FORCE
	"$(MAKE)" -C "$@" NAME="$@" PACKAGE="$@.deli"

$(ZIP): $(PROJS)
	for d in $(TEMP_DIRS); do \
		mkdir -p $$d; \
	done

	sed -e 's|MACRO_VERSION|$(VERSION)|g' \
		legacy.json > $(TEMP_LEGACY)/manifest.json
	mv $(CONTENTS_MODS) $(TEMP_MODS)/

	cd $(TEMP); \
	zip -9r ../$(ZIP) .

	rm -r $(TEMP)

clean:
	rm -f $(ZIP)
