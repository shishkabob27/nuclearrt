@echo off
setlocal

set BUILD_DIR=build\windows

:: Create the build directory if it doesn't exist
if not exist %BUILD_DIR% mkdir %BUILD_DIR%

:: Move to the build directory
cd %BUILD_DIR%

:: Configure with CMake
echo Configuring for Windows...
cmake ..\..

:: Build
echo Building for Windows...
cmake --build . --config Debug

echo Build complete. Binary is located in %BUILD_DIR%\bin\Debug\