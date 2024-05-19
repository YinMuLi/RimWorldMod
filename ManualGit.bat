@echo off
setlocal enabledelayedexpansion
set message=%date% %time:~0,5% AUTO
git add .
git commit -m"!message!"
git push
Pause