#!/bin/bash

here="`dirname \"$0\"`"
buildDir="$here/build"
cd "$here" || exit 1
mkdir -p $buildDir

xcodebuild CONFIGURATION_BUILD_DIR=$buildDir
ditto -ck --rsrc --sequesterRsrc -v --keepParent "$buildDir/QMK Toolbox.app" "$buildDir/QMK.Toolbox.app.zip"
packagesbuild "QMK Toolbox.pkgproj"
mv "$buildDir/QMK Toolbox.pkg" "$buildDir/QMK.Toolbox.pkg"
