#!/bin/bash

# Create build directory based on platform
if [[ "$OSTYPE" == "darwin"* ]]; then
    # macOS
    BUILD_DIR="build/macos"
    PLATFORM="macOS"
elif [[ "$OSTYPE" == "linux-gnu"* ]]; then
    # Linux
    BUILD_DIR="build/linux"
    PLATFORM="Linux"
else
    echo "Unsupported platform: $OSTYPE"
    exit 1
fi

# Create the build directory if it doesn't exist
mkdir -p $BUILD_DIR

# Move to the build directory
cd $BUILD_DIR

# Configure with CMake
echo "Configuring for $PLATFORM..."
cmake ../..

# Build
echo "Building for $PLATFORM..."
cmake --build .

echo "Build complete. Binary is located in $BUILD_DIR/bin/" 