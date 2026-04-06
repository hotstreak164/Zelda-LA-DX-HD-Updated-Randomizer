#!/bin/sh
# Finalization script for the LADXHD Patcher — Linux target.
# Run by the patcher under Wine after all game files have been patched.
#
# Arguments:
#   $1  BASE  — absolute path to the game folder
#   $2  NAME  — executable name without extension

set -e

BASE="$1"
NAME="$2"

chmod +x "$BASE/$NAME"

# Make the Launcher executable if it's found in the game dir.
[ -f "$BASE/Launcher" ] && chmod +x "$BASE/Launcher"

# Signal completion to the patcher (which cannot reliably wait on Wine-spawned processes).
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
touch "$SCRIPT_DIR/finalize.done"
