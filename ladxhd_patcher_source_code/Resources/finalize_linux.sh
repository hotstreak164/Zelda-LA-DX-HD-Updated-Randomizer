#!/bin/sh
# Finalization script for the LADXHD Patcher — Linux target.
# Run by the patcher under Wine after all game files have been patched.
#
# Arguments:
#   $1  BASE  — absolute path to the game folder
#   $2  NAME  — executable name without extension

set -e

chmod +x "$1/$2"

# Signal completion to the patcher (which cannot reliably wait on Wine-spawned processes).
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
touch "$SCRIPT_DIR/finalize.done"
