# 🍎 Patching and building on MacOS

## Using the Windows patcher

The patcher available on the [Releases](https://github.com/BigheadSMZ/Zelda-LA-DX-HD-Updated/releases) page can be run with [Wine](https://www.winehq.org/).

> [!Note]
> No extra Wine setup should be required, but depending on the Wine distribution you are using you might need to install [wine-mono](https://github.com/wine-mono/wine-mono/releases/latest) using their .msi if the patcher fails to start.

When [running the patcher](https://github.com/BigheadSMZ/Zelda-LA-DX-HD-Updated?tab=readme-ov-file#patching-v100-or-v114-to-v173) choose `MacOS` as the platform, and `OpenGL` as the target. The patcher should take care of signing the binaries and making them executable, as well as creating .app bundles ready to launch or move to `/Applications`:

```
📁 "Link's Awakening DX HD.app"            # game app
📁 "Link's Awakening DX HD Launcher.app".  # launcher app
⚙️ "Link's Awakening DX HD"                # game binary
⚙️ Launcher                                # launcher binary
```

> [!Note]
> The launcher .app already contains the game, if you choose to use the launcher you can ignore the other .app.

If the patcher fails to perform these steps and the resulting files aren't executable, you can fix them manually:

```bash
# sign / make executable the game binary
$ codesign --force --sign - "Link's Awakening DX HD"
$ chmod +x "Link's Awakening DX HD"

# same for the launcher binary
$ codesign --force --sign - Launcher
$ chmod +x Launcher

# dynamic libraries should also be signed
$ codesign --force --sign - *.dylib
```

You are good to go!

## Building from source

### Requirements
* [dotnet8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) (`$ dotnet --list-sdks` should return a `8.0.*` version).
* [Wine](https://www.winehq.org/) (required to compile MonoGame effects/shaders).

### Setup
* [Setup MonoGame effects compilation](https://docs.monogame.net/articles/getting_started/1_setting_up_your_os_for_development_macos.html?tabs=android#setup-wine-for-effect-compilation):
```bash
$ wget -qO- https://monogame.net/downloads/net9_mgfxc_wine_setup.sh | bash
```
* Update game assets following the [README instructions](https://github.com/BigheadSMZ/Zelda-LA-DX-HD-Updated?tab=readme-ov-file#updating-source-code-assets).

### Building
From the `ladxhd_game_source_code` directory, build the macOS project specifying your hardware architecture:
```bash
# arm64 / Apple Silicon
$ dotnet publish ProjectZ.MacOS/ProjectZ.MacOS.csproj -c Release -f net8.0 -r osx-arm64 -p:PublishProfile=FolderProfile_MacOS

# x64 / Intel
$ dotnet publish ProjectZ.MacOS/ProjectZ.MacOS.csproj -c Release -f net8.0 -r osx-x64 -p:PublishProfile=FolderProfile_MacOS_x64
```

The resulting ready-to-run binaries will be available in `ladxhd_game_source_code/~Publish/MacOS-Arm64` or `ladxhd_game_source_code/~Publish/MacOS-x86_64`.

## Generating an application bundle

The build project accepts a `CreateAppBundle` parameter that will yield a full-fledged application ready to be moved into the `/Applications` directory.

> [!Note]
> Since the application is not signed / notarized, it won't be usable outside the host where it's been built without removing the macOS quarantine flag.

```bash
# arm64 / Apple Silicon
$ dotnet publish ProjectZ.MacOS/ProjectZ.MacOS.csproj -c Release -f net8.0 -r osx-arm64 -p:PublishProfile=FolderProfile_MacOS -p:CreateAppBundle=true

# x64 / Intel
$ dotnet publish ProjectZ.MacOS/ProjectZ.MacOS.csproj -c Release -f net8.0 -r osx-x64 -p:PublishProfile=FolderProfile_MacOS_x64 -p:CreateAppBundle=true
```

The resulting application will be available in `ladxhd_game_source_code/~Publish/MacOS-Arm64` or `ladxhd_game_source_code/~Publish/MacOS-x86_64`.
