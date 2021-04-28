include makefile.var

# Local
NAME                 = Deli.H3VR
MOD                  = $(NAME).deli
TEMP                 = temp

.PHONY: FORCE all clean

all: $(NAME)
FORCE:

$(NAME): FORCE
	"$(MAKE)" -C "$@" NAME="$@" MOD="$@.deli"
