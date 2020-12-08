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
NAME      = Deli.H3VR
ZIP       = $(NAME).zip
TEMP      = temp

PROJ_ROOT = $(NAME)
PROJS     = $(PROJ_ROOT)

TEMP_MODS = temp/mods
CONTENTS_MODS  = $(addsuffix /*.deli,$(PROJS))

.PHONY: FORCE all clean

all: clean $(ZIP)
FORCE:

$(PROJS): FORCE
	"$(MAKE)" -C "$@" NAME="$@" PACKAGE="$@.deli"

$(ZIP): $(PROJS)
	mkdir -p $(TEMP_MODS)
	
	mv $(CONTENTS_MODS) $(TEMP_MODS)/

	cd $(TEMP); \
	zip -9r ../$(ZIP) .

	rm -r $(TEMP)

clean:
	rm -f $(ZIP)
