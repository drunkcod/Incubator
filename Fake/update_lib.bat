@echo off
Lib\Fake Build.compile
if ERRORLEVEL 1 goto error
  copy /Y Build\Fake.exe Lib > nul:
  copy /Y Build\Fake.Core.dll Lib > nul:
exit /B 0
:error
echo Update Failed.
