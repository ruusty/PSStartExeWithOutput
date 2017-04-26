import-module R:\Projects-Ruusty\PSStartExeWithOutput\StartExeWithOutput\StartExeWithOutput\bin\Release\StartExeWithOutput.dll -verbose

Start-ExeWithOutput -filepath "sqlplus.exe" -verbose
start-transcript rgh.txt -append

  $(get-module Ruusty.Powershell).ExportedCommands.Keys |% {get-help $_}
