# NuclearRT

NuclearRT is a fast, open-source, cross-platform C++ runtime for Clickteam Fusion 2.5.

> [!CAUTION]
> This project is still in development and is not ready for general use.

## Features
- Faster than the default runtime.
- Provides 64-bit support
- Native Linux & macOS support

## Limitations
- Any missing extension will need to be rewritten.
- Sub-Applications are not supported.

## Supported Platforms
- Windows
- Linux
- macOS
- Web

## Requirements
- Clickteam Fusion 2.5 R295.10 or higher
- CMake 3.14 or higher
- A C++ compiler, such as MSVC or GCC
- Emscripten (for Web builds)

## Usage

Release builds are still not ready. You can build the application manually by following the instructions in the [Development](#development) section.

## Development

Working on NuclearRT has a bit of a strange workflow. The easiest way to do it is:
1. Clone the repository.
2. Create a Symbolic Link for the exporter from `nuclearrt\exporter\bin\Debug\net8.0-windows` to `Clickteam Fusion 2.5\Data\Runtime\nuclearrt\exporter`
3. Create a Symbolic Link for the base runtime from `nuclearrt\runtime` to `Clickteam Fusion 2.5\Data\Runtime\nuclearrt\runtime`

If you make any changes to the exporter:
1. `dotnet build` in the `exporter` directory.
2. Build the application in Fusion.

If you make any changes to the base runtime:
1. Build the application in Fusion.

## Contributing

Contributions are welcome! Please open an issue or pull request to contribute.

## License

This project is licensed under the GPL-3.0 license. See the [LICENSE](LICENSE) file for details.

## Credits

- [Clickteam](https://www.clickteam.com/) for making Fusion.
- [MP2](https://www.mp2.dk/) for making Chowdren and inspiring me to make this runtime and making the Fusion plugin.
- [CTFAK](https://github.com/CTFAK) for making the decompiler used in this project.
