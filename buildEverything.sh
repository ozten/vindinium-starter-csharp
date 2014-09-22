#!/bin/bash
./nuget.exe restore Vindinium.sln
xbuild Vindinium.sln
mdoc-update Vindinium/bin/Debug/Vindinium.dll --import Vindinium/bin/Debug/Vindinium.xml --out xml
mdoc-export-html xml/ -o en
gendarme Vindinium/bin/Debug/Vindinium.dll --xml xml/gendarme.xml --html en/gendarme.html
