@rem Build a release
@setlocal
cd /d %~dp0
call psake build.psake.ps1 -properties "@{verbose=$False;ProjMajorMinor='1.0'}" -parameters "@{VerbosePreference='Continue';DebugPreference='Continue'}" %*
endlocal

