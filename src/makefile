include makefile.var

# Local
NAME                 = Deli.H3VR
MOD                  = $(NAME).deli
ZIP                  = $(NAME).zip
TEMP                 = temp

TEMP_MODS            = temp/Deli/mods
TEMP_LEGACY          = $(TEMP_MODS)/legacy
TEMP_VIRTUAL_OBJECTS = $(TEMP_LEGACY)/VirtualObjects
TEMP_DIRS            = $(TEMP_MODS) $(TEMP_VIRTUAL_OBJECTS)

.PHONY: FORCE all clean

all: clean $(ZIP)
FORCE:

$(NAME): FORCE
	"$(MAKE)" -C "$@" NAME="$@" MOD="$@.deli"

$(ZIP): $(NAME)
	for d in $(TEMP_DIRS); do \
		mkdir -p $$d; \
	done

	sed -e 's|MACRO_VERSION|$(VERSION)|g' \
		legacy.json > $(TEMP_LEGACY)/manifest.json
	mv $(NAME)/$(MOD) $(TEMP_MODS)/

	cd $(TEMP); \
	zip -9r ../$(ZIP) .

	rm -r $(TEMP)

clean:
	rm -f $(ZIP)
