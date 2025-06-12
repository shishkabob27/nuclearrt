cmake_minimum_required(VERSION 3.14)
project(nuclearrt-runtime)

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

# Find SDL2 package using our custom FindSDL2.cmake module
find_package(SDL2 REQUIRED)
find_package(SDL2_image REQUIRED)
find_package(SDL2_mixer REQUIRED)
find_package(SDL2_ttf REQUIRED)

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
    ${IMGUI_PATH}/backends/imgui_impl_sdl2.cpp
    ${IMGUI_PATH}/backends/imgui_impl_sdlrenderer2.cpp
)

# Include directories
include_directories(
    ${CMAKE_CURRENT_SOURCE_DIR}/include
    ${SDL2_INCLUDE_DIR}
    ${SDL2_IMAGE_INCLUDE_DIR}
    ${SDL2_MIXER_INCLUDE_DIR}
    ${SDL2_TTF_INCLUDE_DIR}
    ${IMGUI_PATH}
    ${IMGUI_PATH}/backends
)

# Automatically include all source and header files
file(GLOB_RECURSE SOURCES "${CMAKE_CURRENT_SOURCE_DIR}/source/*.cpp")
file(GLOB_RECURSE HEADERS "${CMAKE_CURRENT_SOURCE_DIR}/include/*.h")

# Create executable
add_executable(${PROJECT_NAME} ${SOURCES} ${HEADERS} ${IMGUI_SOURCES})

# Link libraries
target_link_libraries(${PROJECT_NAME} PRIVATE 
    ${SDL2_LIBRARY}
    ${SDL2_IMAGE_LIBRARY}
    ${SDL2_MIXER_LIBRARY}
    ${SDL2_TTF_LIBRARY}
)

# Set output directory
set_target_properties(${PROJECT_NAME} PROPERTIES
    RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin"
)

# Copy assets to build directory
set(ASSETS_SOURCE_DIR "${CMAKE_CURRENT_SOURCE_DIR}/assets")

# Set assets destination based on platform
if(MSVC)
    # For Visual Studio builds, account for Debug/Release subdirectories
    set(ASSETS_DEST_DIR "${CMAKE_BINARY_DIR}/bin/$<CONFIG>/assets")
else()
    set(ASSETS_DEST_DIR "${CMAKE_BINARY_DIR}/bin/assets")
endif()

# Check if assets directory exists and copy it
if(EXISTS ${ASSETS_SOURCE_DIR})
    # Create a custom command to copy assets after build
    add_custom_command(
        TARGET ${PROJECT_NAME} POST_BUILD
        COMMAND ${CMAKE_COMMAND} -E copy_directory
        ${ASSETS_SOURCE_DIR} ${ASSETS_DEST_DIR}
        COMMENT "Copying assets to build directory"
    )
    message(STATUS "Assets will be copied from ${ASSETS_SOURCE_DIR} to ${ASSETS_DEST_DIR}")
else()
    message(WARNING "Assets directory not found at ${ASSETS_SOURCE_DIR}")
endif()

# Install target
install(TARGETS ${PROJECT_NAME}
    RUNTIME DESTINATION bin
)

# Additional platform-specific linking
if(WIN32)
    # Windows may need additional libraries
    target_link_libraries(${PROJECT_NAME} PRIVATE winmm)
elseif(APPLE)
    # macOS specific frameworks if needed
    # Note: Cocoa framework is already included by our FindSDL2.cmake module
elseif(UNIX AND NOT APPLE)
    # Linux may need additional libraries
    target_link_libraries(${PROJECT_NAME} PRIVATE X11 GL m pthread)
endif()