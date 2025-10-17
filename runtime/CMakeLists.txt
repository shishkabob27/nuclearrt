cmake_minimum_required(VERSION 3.14)
project(nuclearrt-runtime CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Platform-specific compiler flags
if(MSVC)
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} /wd4102")
elseif(NOT EMSCRIPTEN)
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -Wall -Wextra -Wunused-label")
endif()

# Add cmake modules path
set(CMAKE_MODULE_PATH ${CMAKE_MODULE_PATH} "${CMAKE_SOURCE_DIR}/cmake")

if (EMSCRIPTEN)
  add_compile_options(-pthread)
  add_link_options(-sMIN_WEBGL_VERSION=2 -sMAX_WEBGL_VERSION=2 -sALLOW_MEMORY_GROWTH=1 -sINITIAL_MEMORY=2gb -sMAXIMUM_MEMORY=2gb -sOFFSCREENCANVAS_SUPPORT=1 -sPTHREAD_POOL_SIZE_STRICT=0 -sFORCE_FILESYSTEM=1 -sWASMFS=1 -sEXIT_RUNTIME=1 -sUSE_HARFBUZZ=1)
endif()

include(FetchContent)
function(include_dependency libName gitURL gitTag)
    # setup the declare
    FetchContent_Declare(${libName}
            GIT_REPOSITORY ${gitURL}
            GIT_TAG        ${gitTag}
            GIT_SHALLOW    FALSE
            GIT_PROGRESS   TRUE
    )

    FetchContent_MakeAvailable(${libName})
endfunction()

find_package(SDL3 QUIET)
if (NOT SDL3_FOUND)
    message(STATUS "Getting SDL3 from Github")
    include_dependency(SDL3 https://github.com/libsdl-org/SDL.git release-3.2.16)
else()
    message(STATUS "Using local SDL3")
endif()

set(SDLIMAGE_VENDORED ON)
set(SDLIMAGE_AVIF OFF)
set(SDLIMAGE_BMP OFF)
set(SDLIMAGE_JPEG OFF)
set(SDLIMAGE_WEBP OFF)
set(SDLIMAGE_TIF OFF)

find_package(SDL3_image QUIET)
if (NOT SDL3_image_FOUND)
    message(STATUS "Getting SDL3_image from Github")
    include_dependency(SDL3_image https://github.com/libsdl-org/SDL_image.git release-3.2.4)
else()
    message(STATUS "Using local SDL3_image")
endif()

set(SDLTTF_VENDORED ON)

find_package(SDL3_ttf QUIET)
if (NOT SDL3_ttf_FOUND)
    message(STATUS "Getting SDL3_ttf from Github")
    include_dependency(SDL3_ttf https://github.com/libsdl-org/SDL_ttf.git release-3.2.2)
else()
    message(STATUS "Using local SDL3_ttf")
endif()

add_compile_definitions(NUCLEAR_BACKEND_SDL3)

if(WIN32)
    add_compile_definitions(PLATFORM_WINDOWS)
elseif(APPLE)
    add_compile_definitions(PLATFORM_MACOS)
elseif(UNIX AND NOT APPLE AND NOT EMSCRIPTEN)
    add_compile_definitions(PLATFORM_LINUX)
elseif(EMSCRIPTEN)
    add_compile_definitions(PLATFORM_WEB)
endif()

# Configure debug mode
if(CMAKE_BUILD_TYPE STREQUAL "Debug")
    add_compile_definitions(_DEBUG)
endif()

# Dear ImGui setup
if(NOT EMSCRIPTEN)
    set(IMGUI_PATH "${CMAKE_CURRENT_SOURCE_DIR}/libs/imgui")
    set(IMGUI_SOURCES
        ${IMGUI_PATH}/imgui.cpp
        ${IMGUI_PATH}/imgui_demo.cpp
        ${IMGUI_PATH}/imgui_draw.cpp
        ${IMGUI_PATH}/imgui_tables.cpp
        ${IMGUI_PATH}/imgui_widgets.cpp
        ${IMGUI_PATH}/backends/imgui_impl_sdl3.cpp
        ${IMGUI_PATH}/backends/imgui_impl_sdlrenderer3.cpp
    )
    
    # Include directories for ImGui
    include_directories(
        ${IMGUI_PATH}
        ${IMGUI_PATH}/backends
    )
else()
    set(IMGUI_SOURCES "")
endif()

# Include directories
include_directories(
    ${CMAKE_CURRENT_SOURCE_DIR}/include
)

# Automatically include all source and header files
file(GLOB_RECURSE SOURCES "${CMAKE_CURRENT_SOURCE_DIR}/source/*.cpp")
file(GLOB_RECURSE HEADERS "${CMAKE_CURRENT_SOURCE_DIR}/include/*.h")

# Create executable
add_executable(${PROJECT_NAME} ${SOURCES} ${HEADERS} ${IMGUI_SOURCES})

# Platform-specific linking
if(NOT EMSCRIPTEN)
    target_include_directories(${PROJECT_NAME} PRIVATE ${SDL3_INCLUDE_DIRS})
    target_include_directories(${PROJECT_NAME} PRIVATE ${SDL3_image_INCLUDE_DIRS})
    target_include_directories(${PROJECT_NAME} PRIVATE ${SDL3_ttf_INCLUDE_DIRS})
endif()

target_link_libraries(${PROJECT_NAME} PRIVATE 
    SDL3::SDL3
    SDL3_image::SDL3_image
    SDL3_ttf::SDL3_ttf
)

target_link_libraries(${PROJECT_NAME} PUBLIC
    SDL3::Headers
)

# Set output directory
if(EMSCRIPTEN)
    set_target_properties(${PROJECT_NAME} PROPERTIES
    RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/web"
    SUFFIX ".html"
)
else()
    set_target_properties(${PROJECT_NAME} PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin"
    )
endif()

# Copy assets.pak to build directory
set(ASSETS_SOURCE_FILE "${CMAKE_CURRENT_SOURCE_DIR}/assets.pak")

# Set assets destination based on platform
if(MSVC)
    set(ASSETS_DEST_FILE "${CMAKE_BINARY_DIR}/bin/$<CONFIG>/assets.pak")
else()
    set(ASSETS_DEST_FILE "${CMAKE_BINARY_DIR}/bin/assets.pak")
endif()

# Check if assets.pak exists and copy it
if(EXISTS ${ASSETS_SOURCE_FILE} AND NOT EMSCRIPTEN)
    # Create a custom command to copy assets after build
    add_custom_command(
        TARGET ${PROJECT_NAME} POST_BUILD
        COMMAND ${CMAKE_COMMAND} -E copy
        ${ASSETS_SOURCE_FILE} ${ASSETS_DEST_FILE}
        COMMENT "Copying assets.pak to build directory"
    )
    message(STATUS "assets.pak will be copied from ${ASSETS_SOURCE_FILE} to ${ASSETS_DEST_FILE}")
else()
    message(WARNING "assets.pak not found at ${ASSETS_SOURCE_FILE}")
endif()

if(EMSCRIPTEN)
    target_link_options(${PROJECT_NAME} PRIVATE --embed-file ${ASSETS_SOURCE_FILE}@assets.pak)
endif()

#copy SDL3 dlls
if(WIN32 AND NOT EMSCRIPTEN)
    message(STATUS "Copying SDL3 dlls to output directory")
    foreach(sdl_target IN ITEMS SDL3::SDL3 SDL3_image::SDL3_image SDL3_ttf::SDL3_ttf)
        if(TARGET ${sdl_target})
            add_custom_command(
                TARGET ${PROJECT_NAME} POST_BUILD
                COMMAND ${CMAKE_COMMAND} -E copy_if_different
                    $<TARGET_FILE:${sdl_target}>
                    $<TARGET_FILE_DIR:${PROJECT_NAME}>
            )
        endif()
    endforeach()
endif()

# Install target
install(TARGETS ${PROJECT_NAME}
    RUNTIME DESTINATION bin
)

# Additional platform-specific linking
if(WIN32 AND NOT EMSCRIPTEN)
    target_link_libraries(${PROJECT_NAME} PRIVATE winmm)
elseif(UNIX AND NOT APPLE AND NOT EMSCRIPTEN)
    target_link_libraries(${PROJECT_NAME} PRIVATE X11 GL m pthread)
endif()