@echo off
fsi fakefile.fsx
if ERRORLEVEL 1 goto error
  copy /Y Build\Fake.Core.dll Lib
exit /B 0
:error
echo Update Failed.
