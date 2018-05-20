setlocal
call "C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\Tools\VsMSBuildCmd.bat"
msbuild StartExeWithOutput.sln /t:StartExeWithOutput:Clean;StartExeWithOutput:Rebuild /p:Configuration=Release /p:Platform="Any CPU"
endlocal
