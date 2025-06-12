# Locate SDL2_mixer
# This module defines
# SDL2_MIXER_LIBRARY, the name of the library to link against
# SDL2_MIXER_FOUND, if false, do not try to link to SDL2_mixer
# SDL2_MIXER_INCLUDE_DIR, where to find SDL_mixer.h
#
# This module responds to the the flag:
# SDL2_MIXER_BUILDING_LIBRARY
# If this is defined, then no SDL2_mixer main will be linked in because
# only applications need main().
# Otherwise, it is assumed you are building an application and this
# module will attempt to locate and set the the proper link flags
# as part of the returned SDL2_MIXER_LIBRARY variable.
#
# $SDL2DIR is an environment variable that would
# correspond to the ./configure --prefix=$SDL2DIR
# used in building SDL2_mixer.

SET(SDL2_MIXER_SEARCH_PATHS
    ~/Library/Frameworks
    /Library/Frameworks
    /usr/local
    /usr
    /sw # Fink
    /opt/local # DarwinPorts
    /opt/csw # Blastwave
    /opt
    ${SDL2_DIR}
    ${SDL2_MIXER_DIR}
    $ENV{SDL2DIR}
    $ENV{SDL2_MIXER_DIR}
)

FIND_PATH(SDL2_MIXER_INCLUDE_DIR SDL_mixer.h
    HINTS
    $ENV{SDL2MIXERDIR}
    PATH_SUFFIXES include/SDL2 include
    PATHS ${SDL2_MIXER_SEARCH_PATHS}
)

if(CMAKE_SIZEOF_VOID_P EQUAL 8) 
    set(PATH_SUFFIXES lib64 lib/x64 lib)
else() 
    set(PATH_SUFFIXES lib/x86 lib)
endif() 

FIND_LIBRARY(SDL2_MIXER_LIBRARY
    NAMES SDL2_mixer
    HINTS
    $ENV{SDL2MIXERDIR}
    PATH_SUFFIXES ${PATH_SUFFIXES}
    PATHS ${SDL2_MIXER_SEARCH_PATHS}
)

if(SDL2_MIXER_INCLUDE_DIR AND EXISTS "${SDL2_MIXER_INCLUDE_DIR}/SDL_mixer.h")
  file(STRINGS "${SDL2_MIXER_INCLUDE_DIR}/SDL_mixer.h" SDL2_MIXER_VERSION_MAJOR_LINE REGEX "^#define[ \t]+SDL_MIXER_MAJOR_VERSION[ \t]+[0-9]+$")
  file(STRINGS "${SDL2_MIXER_INCLUDE_DIR}/SDL_mixer.h" SDL2_MIXER_VERSION_MINOR_LINE REGEX "^#define[ \t]+SDL_MIXER_MINOR_VERSION[ \t]+[0-9]+$")
  file(STRINGS "${SDL2_MIXER_INCLUDE_DIR}/SDL_mixer.h" SDL2_MIXER_VERSION_PATCH_LINE REGEX "^#define[ \t]+SDL_MIXER_PATCHLEVEL[ \t]+[0-9]+$")
  string(REGEX REPLACE "^#define[ \t]+SDL_MIXER_MAJOR_VERSION[ \t]+([0-9]+)$" "\\1" SDL2_MIXER_VERSION_MAJOR "${SDL2_MIXER_VERSION_MAJOR_LINE}")
  string(REGEX REPLACE "^#define[ \t]+SDL_MIXER_MINOR_VERSION[ \t]+([0-9]+)$" "\\1" SDL2_MIXER_VERSION_MINOR "${SDL2_MIXER_VERSION_MINOR_LINE}")
  string(REGEX REPLACE "^#define[ \t]+SDL_MIXER_PATCHLEVEL[ \t]+([0-9]+)$" "\\1" SDL2_MIXER_VERSION_PATCH "${SDL2_MIXER_VERSION_PATCH_LINE}")
  set(SDL2_MIXER_VERSION_STRING ${SDL2_MIXER_VERSION_MAJOR}.${SDL2_MIXER_VERSION_MINOR}.${SDL2_MIXER_VERSION_PATCH})
  unset(SDL2_MIXER_VERSION_MAJOR_LINE)
  unset(SDL2_MIXER_VERSION_MINOR_LINE)
  unset(SDL2_MIXER_VERSION_PATCH_LINE)
endif()

INCLUDE(FindPackageHandleStandardArgs)

FIND_PACKAGE_HANDLE_STANDARD_ARGS(SDL2_mixer
                                  REQUIRED_VARS SDL2_MIXER_LIBRARY SDL2_MIXER_INCLUDE_DIR
                                  VERSION_VAR SDL2_MIXER_VERSION_STRING) 