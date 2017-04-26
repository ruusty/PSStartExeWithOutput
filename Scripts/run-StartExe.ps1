$(get-module StartExeWithOutput).ExportedCommands.Keys | % { get-help $_ }

import-module $(join-path $PSScriptRoot "..\StartExeWithOutput\StartExeWithOutput\bin\Release\StartExeWithOutput.dll") -verbose

start-transcript rgh.txt -append
Start-ExeWithOutput -filepath "sqlplus.exe" -ArgumentList @("arg01","arg02")-verbose
Stop-Transcript
Remove-Module StartExeWithOutput

timeout /t 10
