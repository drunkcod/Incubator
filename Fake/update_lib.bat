@echo off
Lib\Fake Build.compile
if ERRORLEVEL 1 goto error
  copy /Y Build\* Lib > nul:
exit /B 0
:error
echo Update Failed.
