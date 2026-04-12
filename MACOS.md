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

If the patcher fails to perform these steps and the resulting files aren't executable, you can fix them manually (if you also want to create the app bundles, check [Creating .app bundles manually](#creating-app-bundles-manually)):

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


## Creating .app bundles manually

The patcher will generate ready to use .app bundles for game and launcher when ran via Wine on macOS. When that setup is not possible and the game is patched from a different platform, it is still possible to create macOS apps once the files are available on a macOS host. Here is the script used by the patcher slightly adapted to be executed manually. The script takes the path to the `Links Awakening DX HD` directory as a single parameter, or can be called without parameters when invoked from inside that directory (defaults to `.`).

```bash
#!/bin/sh

set -e

# Change to sync with patcher / game version.
VERSION="1.7.4"

CURRENT_DIR=$(pwd)
TMP_DIR=$(mktemp -d 2>/dev/null || mktemp -d -t 'ladxhd-app-bundle')
BASE=$(realpath "${1:-.}")

NAME="Link's Awakening DX HD"
BUNDLE="$BASE/$NAME.app"
BUNDLE_TMP="$TMP_DIR/$NAME.app"

cleanup() {
    cd "$CURRENT_DIR"
    rm -rf "$TMP_DIR"
}

trap cleanup EXIT

# Set executable bit on the standalone binary.
chmod +x "$BASE/$NAME"

# Ad-hoc codesign executable files (binary and dylibs).
codesign --sign - --force "$BASE/$NAME"
for dylib in libopenal.dylib libSDL2-2.0.0.dylib; do
    [ -f "$BASE/$dylib" ] && codesign --sign - --force "$BASE/$dylib"
done

# Create bundle directory structure inside temp directory.
# The bundle is only moved into BASE once all steps succeed , so a failure
# at any point leaves nothing malformed in the game directory.
mkdir -p "$BUNDLE_TMP/Contents/MacOS"
mkdir -p "$BUNDLE_TMP/Contents/Resources"

# Copy the signed binary and dylibs into Contents/MacOS/ preserving permissions.
cp -p "$BASE/$NAME" "$BUNDLE_TMP/Contents/MacOS/$NAME"
for dylib in libopenal.dylib libSDL2-2.0.0.dylib; do
    [ -f "$BASE/$dylib" ] && cp -p "$BASE/$dylib" "$BUNDLE_TMP/Contents/MacOS/$dylib"
done

# Copy Data (excluding Backup), Content, and Mods into Contents/MacOS/.
if [ -d "$BASE/Data" ]; then
    cp -rp "$BASE/Data" "$BUNDLE_TMP/Contents/MacOS/Data"
    rm -rf "$BUNDLE_TMP/Contents/MacOS/Data/Backup"
fi
[ -d "$BASE/Content" ] && cp -rp "$BASE/Content" "$BUNDLE_TMP/Contents/MacOS/Content"
[ -d "$BASE/Mods" ] && cp -rp "$BASE/Mods" "$BUNDLE_TMP/Contents/MacOS/Mods"

# MonogGame expects Content to be placed inside Contents/Resources, while game code
# expects Content to be placed alongside the binary.
# Create a Content symlink so MonoGame and game code finds assets via both search paths.
ln -sf "../MacOS/Content" "$BUNDLE_TMP/Contents/Resources/Content"

# Download / create bundle-specific resources.
curl -sL -o "$BUNDLE_TMP/Contents/Resources/Icon.icns" \
    "https://github.com/BigheadSMZ/Zelda-LA-DX-HD-Updated/raw/refs/heads/main/ladxhd_patcher_source_code/Resources/Icon.icns"

cat <<EOF >"$BUNDLE_TMP/Contents/Info.plist"
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleDevelopmentRegion</key>
    <string>en</string>
    <key>CFBundleExecutable</key>
    <string>${NAME}</string>
    <key>CFBundleIconFile</key>
    <string>Icon</string>
    <key>CFBundleIdentifier</key>
    <string>com.projectz.game</string>
    <key>CFBundleInfoDictionaryVersion</key>
    <string>6.0</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>CFBundleShortVersionString</key>
    <string>${VERSION}</string>
    <key>CFBundleSignature</key>
    <string>FONV</string>
    <key>CFBundleVersion</key>
    <string>${VERSION}</string>
    <key>LSApplicationCategoryType</key>
    <string>public.app-category.games</string>
    <key>LSMinimumSystemVersion</key>
    <string>10.15</string>
    <key>NSPrincipalClass</key>
    <string>NSApplication</string>
    <key>LSRequiresNativeExecution</key>
    <true/>
    <key>LSArchitecturePriority</key>
    <array>
        <string>$(uname -m)</string>
    </array>
</dict>
</plist>
EOF

# Codesign the app bundle.
codesign --sign - --force --deep "$BUNDLE_TMP"

# Atomically move the completed bundle into BASE, replacing any stale copy.
rm -rf "$BUNDLE"
mv "$BUNDLE_TMP" "$BUNDLE"

# If the Launcher binary is present, make it executable and create its own app bundle.
# The Launcher app bundle includes the game to allow launching it easily.
if [ -f "$BASE/Launcher" ]; then
    chmod +x "$BASE/Launcher"

    codesign --sign - --force "$BASE/Launcher"
    for dylib in libAvaloniaNative.dylib libHarfBuzzSharp.dylib libSkiaSharp.dylib; do
        [ -f "$BASE/$dylib" ] && codesign --sign - --force "$BASE/$dylib"
    done

    LAUNCHER_BUNDLE="$BASE/$NAME Launcher.app"
    LAUNCHER_TMP="$TMP_DIR/$NAME Launcher.app"

    # Copy the completed game bundle as the foundation (includes all game data, Content, Mods).
    cp -rp "$BUNDLE" "$LAUNCHER_TMP"

    # Add the Launcher binary and its Avalonia/Skia dylibs on top.
    cp -p "$BASE/Launcher" "$LAUNCHER_TMP/Contents/MacOS/Launcher"
    for dylib in libAvaloniaNative.dylib libHarfBuzzSharp.dylib libSkiaSharp.dylib; do
        [ -f "$BASE/$dylib" ] && cp -p "$BASE/$dylib" "$LAUNCHER_TMP/Contents/MacOS/$dylib"
    done

    # Write a launcher-specific Info.plist derived from the game bundle's plist.
    # Substitutes CFBundleExecutable (the game name) with "Launcher",
    # and changes CFBundleIdentifier from com.projectz.game to com.projectz.launcher.
    sed -e "s|<string>${NAME}</string>|<string>Launcher</string>|g" \
        -e "s|com\.projectz\.game|com.projectz.launcher|g" \
        "$BUNDLE/Contents/Info.plist" >"$LAUNCHER_TMP/Contents/Info.plist"

    # Codesign the app bundle.
    codesign --sign - --force --deep "$LAUNCHER_TMP"

    # Atomically move the completed launcher bundle into BASE, replacing any stale copy.
    rm -rf "$LAUNCHER_BUNDLE"
    mv "$LAUNCHER_TMP" "$LAUNCHER_BUNDLE"
fi
```
