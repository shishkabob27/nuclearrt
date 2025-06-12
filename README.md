# NuclearRT

NuclearRT is a custom High-performance C++ runtime for Clickteam Fusion 2.5.

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

## Usage

1. Download and install the latest release from the [releases page](https://github.com/shishkabob27/nuclearrt/releases).
1. Open your Clickteam Fusion 2.5 application.
2. Set build type to `NuclearRT (Source)`
3. Build the application. This will convert the application to C++.
4. Run `build.bat` in the output directory to compile the application.

## Development

Working on NuclearRT has a bit of a strange workflow. The easiest way to do it is:
1. Clone the repository.
2. Create a Symbolic Link for the exporter from `nuclearrt\exporter\bin\Debug\net8.0` to `Clickteam Fusion 2.5\Data\Runtime\NuclearRT\exporter`
3. Create a Symbolic Link for the base runtime from `nuclearrt\runtime` to `Clickteam Fusion 2.5\Data\Runtime\NuclearRT\runtime`

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
- [CTFAK](https://github.com/CTFAK) for making decompiler used in this project.