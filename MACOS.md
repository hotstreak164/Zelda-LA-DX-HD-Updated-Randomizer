# Patching and building on MacOS

## Using the Windows patcher

The patcher available on the [Releases](https://github.com/BigheadSMZ/Zelda-LA-DX-HD-Updated/releases) page can be run with Wine, no special setup is required. When running the patcher choose `MacOS` as the platform, and `OpenGL` as the target. The resulting binary needs to be signed and marked as executable (or macOS will refuse to run it):

```bash
$ codesign --force --sign - "Link's Awakening DX HD"
$ chmod +x "Link's Awakening DX HD"
```

You are good to go! The binary should now run as expected.

## Building from source

### Requirements
* dotnet8 SDK (`$ dotnet --list-sdks` should return a `8.0.*` version)
* wine (required to compile MonoGame effects/shaders)

### Setup
* [Setup MonoGame effects compilation](https://docs.monogame.net/articles/getting_started/1_setting_up_your_os_for_development_macos.html?tabs=android#setup-wine-for-effect-compilation):
```bash
$ wget -qO- https://monogame.net/downloads/net9_mgfxc_wine_setup.sh | bash
```
* Update game assets following the [README instructions](https://github.com/BigheadSMZ/Zelda-LA-DX-HD-Updated?tab=readme-ov-file#updating-source-code-assets).

### Building
From inside the `ladxhd_game_source_code` directory, build the macOS project specifying your hardware architecture:
```bash
# arm64 / Apple Silicon
$ dotnet publish ProjectZ.MacOS/ProjectZ.MacOS.csproj -c Release -f net8.0 -r osx-arm64 -p:PublishProfile=FolderProfile_MacOS

# x64 / Intel
$ dotnet publish ProjectZ.MacOS/ProjectZ.MacOS.csproj -c Release -f net8.0 -r osx-x64 -p:PublishProfile=FolderProfile_MacOS_x64
```

The resulting ready-to-run project will be available in `ladxhd_game_source_code/~Publish/MacOS-Arm64` or `ladxhd_game_source_code/~Publish/MacOS-x86_64`.

## Generating an application bundle

The build project accepts a `CreateAppBundle` parameter that will yield a full-fledged application ready to be moved into the `Applications` directory.

*Note: since the application is not signed, it won't be usable outside the host where it's been built without removing the macOS quarantine flag first.*

```bash
# arm64 / Apple Silicon
$ dotnet publish ProjectZ.MacOS/ProjectZ.MacOS.csproj -c Release -f net8.0 -r osx-arm64 -p:PublishProfile=FolderProfile_MacOS -p:CreateAppBundle=true

# x64 / Intel
$ dotnet publish ProjectZ.MacOS/ProjectZ.MacOS.csproj -c Release -f net8.0 -r osx-x64 -p:PublishProfile=FolderProfile_MacOS_x64 -p:CreateAppBundle=true
```

The resulting application will be available in `ladxhd_game_source_code/~Publish/MacOS-Arm64` or `ladxhd_game_source_code/~Publish/MacOS-x86_64`.
