#!/bin/bash
here="`dirname \"$0\"`"
cd "$here" || exit 1
cp "Build/Products/Release/QMK Toolbox.app.zip" QMK.Toolbox.app.zip
cp "Build/QMK Toolbox.pkg" QMK.Toolbox.pkg