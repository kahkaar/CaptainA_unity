#!/usr/bin/env bash
set -euo pipefail

# dotnet tool install fails in a directory with more than one project file.
cd "$HOME"

CSHARPIER_VERSION=1.3.0
PRETTIER_VERSION=3.9.10

dotnet tool install csharpier --global --version "$CSHARPIER_VERSION"
npm install --global prettier@"$PRETTIER_VERSION"
