include ../makefile.var
CONTENTS += hooks.conf $(NAME).dll ../../../../Deli.H3VR.Patcher/bin/$(CONFIG)/$(FRAMEWORK)/$(NAME).Patcher.dll

.PHONY: all precompile compile postcompile clean

all: clean $(MOD)
build: precompile compile postcompile

$(MOD): build
	zip -9j $@ $(addprefix bin/$(CONFIG)/$(FRAMEWORK)/,$(CONTENTS)) 

precompile:
	tee $(MANIFEST).old < $(MANIFEST)
	sed -i \
		-e 's|MACRO_VERSION|$(VERSION)|g' \
		$(MANIFEST)

compile:
	$(BUILD)

postcompile: $(PACKAGE)
	mv $(MANIFEST_OLD) $(MANIFEST)

clean:
	rm -f $(MOD)
