# NuclearRT Runtime

A cross-platform application using SDL2.

## Prerequisites

- CMake (version 3.14 or higher)
- C++ compiler with C++17 support
- SDL2 development libraries
- SDL2_image development libraries
- SDL2_mixer development libraries

### Installing SDL2 and Extensions

#### Windows
1. Download the development libraries from:
   - SDL2: https://www.libsdl.org/download-2.0.php
   - SDL2_image: https://github.com/libsdl-org/SDL_image/releases
   - SDL2_mixer: https://github.com/libsdl-org/SDL_mixer/releases

2. Extract each to separate folders (e.g., C:\SDL2, C:\SDL2_image, C:\SDL2_mixer)

3. Set environment variables to point to the extracted folders:
   ```
   set SDL2_DIR=C:\SDL2
   set SDL2_IMAGE_DIR=C:\SDL2_image
   set SDL2_MIXER_DIR=C:\SDL2_mixer
   ```

4. Copy all DLL files to your executable directory:
   - SDL2.dll (from C:\SDL2\lib\x64 or x86)
   - SDL2_image.dll (from C:\SDL2_image\lib\x64 or x86)
   - SDL2_mixer.dll (from C:\SDL2_mixer\lib\x64 or x86)
   - All dependency DLLs (libpng, zlib, etc.)

#### macOS
Using Homebrew:
```
brew install sdl2 sdl2_image sdl2_mixer
```

#### Linux (Ubuntu/Debian)
```
sudo apt-get install libsdl2-dev libsdl2-image-dev libsdl2-mixer-dev
```

#### Linux (Fedora)
```
sudo dnf install SDL2-devel SDL2_image-devel SDL2_mixer-devel
```

## Building

### Windows
Run the build script:
```
build.bat
```

### macOS and Linux
Run the build script:
```
chmod +x build.sh
./build.sh
```

### Manual CMake Build

If you prefer to use CMake directly:

```bash
# Create a build directory
mkdir -p build
cd build

# Configure
cmake ..

# Build
cmake --build .
```

## Running the Application

After building, the executable will be in the `bin` directory inside your build folder:

- Windows: `build\windows\bin\Release\{appname}.exe`
- macOS: `build/macos/bin/{appname}`
- Linux: `build/linux/bin/{appname}`