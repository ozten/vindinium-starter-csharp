#!/bin/bash
./nuget.exe restore Vindinium.sln
xbuild Vindinium.sln
mdoc-update Vindinium/bin/Debug/Vindinium.dll --import Vindinium/bin/Debug/Vindinium.xml --out xml
mdoc-export-html xml/ -o en
if [ -z "$(git branch | grep gh-pages)" ]; then
  git branch gh-pages
fi
git checkout gh-pages
git add en
git commit -m "Push new docs"
git push origin master
