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

# 1. Set executable bit and ad-hoc codesign the standalone binary.
chmod +x "$BASE/$NAME"
codesign --sign - --force "$BASE/$NAME"

# 2. Create bundle directory structure inside TempFolder.
#    The bundle is only moved into BASE once all steps succeed (step 7), so a failure
#    at any point leaves nothing malformed in the game folder — TempFolder is cleaned
#    up by the patcher on exit.
mkdir -p "$BUNDLE_TMP/Contents/MacOS"
mkdir -p "$BUNDLE_TMP/Contents/Resources"

# 3. Copy the signed binary and dylibs into Contents/MacOS/ preserving permissions.
#    cp -p carries the Mach-O LC_CODE_SIGNATURE load command into the bundle copy.
cp -p "$BASE/$NAME" "$BUNDLE_TMP/Contents/MacOS/$NAME"
for dylib in libopenal.dylib libSDL2-2.0.0.dylib; do
    [ -f "$BASE/$dylib" ] && cp -p "$BASE/$dylib" "$BUNDLE_TMP/Contents/MacOS/$dylib"
done

# 4. Copy Data (excluding Backup), Content, and Mods into Contents/MacOS/.
#    Data/Backup contains the original Windows PE executable which codesign cannot sign,
#    so it is excluded from the bundle.
cp -rp "$BASE/Data" "$BUNDLE_TMP/Contents/MacOS/Data"
rm -rf "$BUNDLE_TMP/Contents/MacOS/Data/Backup"
[ -d "$BASE/Content" ] && cp -rp "$BASE/Content" "$BUNDLE_TMP/Contents/MacOS/Content"
[ -d "$BASE/Mods"    ] && cp -rp "$BASE/Mods"    "$BUNDLE_TMP/Contents/MacOS/Mods"

# 5. Copy bundle resources written by the patcher into the bundle.
cp "$SCRIPT_DIR/Icon.icns"  "$BUNDLE_TMP/Contents/Resources/Icon.icns"
cp "$SCRIPT_DIR/Info.plist" "$BUNDLE_TMP/Contents/Info.plist"

# 6. Create a Content symlink so MonoGame finds assets via both search paths.
ln -sf "../MacOS/Content" "$BUNDLE_TMP/Contents/Resources/Content"

# 7. Atomically move the completed bundle into BASE, replacing any stale copy.
rm -rf "$BUNDLE"
mv "$BUNDLE_TMP" "$BUNDLE"

# 8. Signal completion to the patcher (which cannot reliably wait on Wine-spawned processes).
touch "$SCRIPT_DIR/finalize.done"
