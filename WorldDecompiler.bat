@echo off
set /p wmap="Enter wmap name: "
copy %wmap%.wmap bin\Debug
bin\Debug\Json2Wmap %wmap%.wmap %wmap%.jm decompile
echo Decompiled map!
echo Moving jm to Worlds Folder
move %wmap%.jm wServer/realm/worlds/
echo Moved jm to Worlds Folder!
pause