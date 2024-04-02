:: Used to automate the process going from .bps patches to WAD files for MMR
:: Calls `modify_bps_patched_z64_for_vc.exe` to inject VC header/functionality,
:: which can be complex to do manually if the ROM is larger than 32MB, and
:: subsequently the standard `wadpacker` CLI

@echo off

echo ^> Running 'modify_bps_patched_z64_for_vc.exe'...

cd "%~dp0"

modify_bps_patched_z64_for_vc.exe %1

if %errorlevel%==0 (
  echo ^> Successfully created '00000005.app'.

  echo ^> Running 'wadpacker.exe'...
  echo ^>^> wadpacker mm.tik mm.tmd mm.cert 'output.wad' -i NMRE

  wadpacker mm.tik mm.tmd mm.cert "output.wad" -i NMRE
) else (
  echo.
  echo Failed to modify BPS-patched z64 for VC - Did you drag the .z64 onto this BATCH file?
)

echo.
pause
