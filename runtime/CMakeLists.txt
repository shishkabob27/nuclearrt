cmake_minimum_required(VERSION 3.14)
project(nuclearrt-runtime CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Platform-specific compiler flags
if(MSVC)
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} /wd4102")
else()
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -Wall -Wextra -Wunused-label")
endif()

# Add cmake modules path
set(CMAKE_MODULE_PATH ${CMAKE_MODULE_PATH} "${CMAKE_SOURCE_DIR}/cmake")

#SDL3
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

# Handle platform-specific settings
if(WIN32)
    # Windows-specific settings
    add_compile_definitions(PLATFORM_WINDOWS)
elseif(APPLE)
    # macOS-specific settings
    add_compile_definitions(PLATFORM_MACOS)
elseif(UNIX AND NOT APPLE)
    # Linux-specific settings
    add_compile_definitions(PLATFORM_LINUX)
endif()

# Configure debug mode
if(CMAKE_BUILD_TYPE STREQUAL "Debug")
    add_compile_definitions(_DEBUG)
endif()

# Dear ImGui setup
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

# Include directories
include_directories(
    ${CMAKE_CURRENT_SOURCE_DIR}/include
    ${IMGUI_PATH}
    ${IMGUI_PATH}/backends
)

# Automatically include all source and header files
file(GLOB_RECURSE SOURCES "${CMAKE_CURRENT_SOURCE_DIR}/source/*.cpp")
file(GLOB_RECURSE HEADERS "${CMAKE_CURRENT_SOURCE_DIR}/include/*.h")

# Create executable
add_executable(${PROJECT_NAME} ${SOURCES} ${HEADERS} ${IMGUI_SOURCES})

target_include_directories(${PROJECT_NAME} PRIVATE ${SDL3_INCLUDE_DIRS})
target_include_directories(${PROJECT_NAME} PRIVATE ${SDL3_image_INCLUDE_DIRS})
target_include_directories(${PROJECT_NAME} PRIVATE ${SDL3_ttf_INCLUDE_DIRS})

# Link libraries using SDL3 targets
target_link_libraries(${PROJECT_NAME} PRIVATE 
    SDL3::SDL3
    SDL3_image::SDL3_image
    SDL3_ttf::SDL3_ttf
)

# Set output directory
set_target_properties(${PROJECT_NAME} PROPERTIES
    RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin"
)

# Copy assets.pak to build directory
set(ASSETS_SOURCE_FILE "${CMAKE_CURRENT_SOURCE_DIR}/assets.pak")

# Set assets destination based on platform
if(MSVC)
    # For Visual Studio builds, account for Debug/Release subdirectories
    set(ASSETS_DEST_FILE "${CMAKE_BINARY_DIR}/bin/$<CONFIG>/assets.pak")
else()
    set(ASSETS_DEST_FILE "${CMAKE_BINARY_DIR}/bin/assets.pak")
endif()

# Check if assets.pak exists and copy it
if(EXISTS ${ASSETS_SOURCE_FILE})
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

# Install target
install(TARGETS ${PROJECT_NAME}
    RUNTIME DESTINATION bin
)

# Additional platform-specific linking
if(WIN32)
    target_link_libraries(${PROJECT_NAME} PRIVATE winmm)
elseif(UNIX AND NOT APPLE)
    target_link_libraries(${PROJECT_NAME} PRIVATE X11 GL m pthread)
endif()