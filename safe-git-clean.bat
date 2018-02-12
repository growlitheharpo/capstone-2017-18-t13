@echo off
echo git clean will remove the following files:
git clean -xd -n -e CloudBuild
echo Please confirm the above list (Y/N)

set INPUT=
set /P INPUT=Type input: %=%

If /I "%INPUT%"=="y" goto yes
goto End


:yes
git clean -xd -f -e CloudBuild
git gc

:End
pause
