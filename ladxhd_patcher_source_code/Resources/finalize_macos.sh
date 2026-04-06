#!/bin/sh
# Finalization script for the LADXHD Patcher — macOS target.
# Run by the patcher under Wine after all game files have been patched.
# All host-specific steps are centralised here to avoid Wine process-synchronisation issues.
#
# Arguments:
#   $1  BASE  — absolute path to the game folder (e.g. /Users/foo/Games/LADXHD)
#   $2  NAME  — executable name without extension (e.g. "Link's Awakening DX HD")

set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
BASE="$1"
NAME="$2"
BUNDLE="$BASE/$NAME.app"
BUNDLE_TMP="$SCRIPT_DIR/$NAME.app"

# Clean slate: remove any leftover temp bundle from a previous failed run.
rm -rf "$BUNDLE_TMP"

# Set executable bit and ad-hoc codesign the standalone binary.
chmod +x "$BASE/$NAME"
codesign --sign - --force "$BASE/$NAME"

# Create bundle directory structure inside TempFolder.
# The bundle is only moved into BASE once all steps succeed , so a failure
# at any point leaves nothing malformed in the game folder — TempFolder is cleaned
# up by the patcher on exit.
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

# Copy bundle resources written by the patcher into the bundle.
cp "$SCRIPT_DIR/Icon.icns" "$BUNDLE_TMP/Contents/Resources/Icon.icns"
cp "$SCRIPT_DIR/Info.plist" "$BUNDLE_TMP/Contents/Info.plist"

# Create a Content symlink so MonoGame finds assets via both search paths.
ln -sf "../MacOS/Content" "$BUNDLE_TMP/Contents/Resources/Content"

# Atomically move the completed bundle into BASE, replacing any stale copy.
rm -rf "$BUNDLE"
mv "$BUNDLE_TMP" "$BUNDLE"

# If the Launcher binary is present, make it executable and create its own app bundle.
# The Launcher app bundle includes the game to allow launching it easily.
if [ -f "$BASE/Launcher" ]; then
    chmod +x "$BASE/Launcher"
    codesign --sign - --force "$BASE/Launcher"

    LAUNCHER_BUNDLE="$BASE/${NAME} Launcher.app"
    LAUNCHER_TMP="$SCRIPT_DIR/${NAME} Launcher.app"
    rm -rf "$LAUNCHER_TMP"

    # Copy the completed game bundle as the foundation (includes all game data, Content, Mods).
    cp -rp "$BUNDLE" "$LAUNCHER_TMP"

    # Add the Launcher binary and its Avalonia/Skia dylibs on top.
    cp -p "$BASE/Launcher" "$LAUNCHER_TMP/Contents/MacOS/Launcher"
    for dylib in libAvaloniaNative.dylib libHarfBuzzSharp.dylib libSkiaSharp.dylib; do
        [ -f "$BASE/$dylib" ] && cp -p "$BASE/$dylib" "$LAUNCHER_TMP/Contents/MacOS/$dylib"
    done

    # Write a launcher-specific Info.plist derived from the game bundle's rendered plist.
    # Substitutes CFBundleExecutable (the game name) with "Launcher",
    # and changes CFBundleIdentifier from com.projectz.game to com.projectz.launcher.
    sed -e "s|<string>${NAME}</string>|<string>Launcher</string>|g" \
        -e "s|com\.projectz\.game|com.projectz.launcher|g" \
        "$BUNDLE/Contents/Info.plist" >"$LAUNCHER_TMP/Contents/Info.plist"

    # Atomically move the completed launcher bundle into BASE, replacing any stale copy.
    rm -rf "$LAUNCHER_BUNDLE"
    mv "$LAUNCHER_TMP" "$LAUNCHER_BUNDLE"
fi

# Signal completion to the patcher (which cannot reliably wait on Wine-spawned processes).
touch "$SCRIPT_DIR/finalize.done"
