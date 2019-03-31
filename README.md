# StartExeWithOutput Powershell Module <!-- omit in toc --> #

~~~text
Project:        Ruusty Powershell Tools
Product:        StartExeWithOutput Powershell Module
Version:        1.0.0.0
Date:           2018-10-07
Description:    StartExeWithOutput Powershell Module with binary cmdlet to run an executable.
~~~

<a name="TOC"></a>

- [Description](#description)
- [Examples](#examples)

## Description ##

A *Powershell* binary cmdlet that

- Returns the exit code.
- Can optionally log Stdout to a file.
- Can optionally log Stderr to a file.
- Display the executables Stdout and Stderr to the console.
- When `-Verbose` display Stdout and Stderr (`start-transcript` will now log it.)
- Throw on invalid exit code

~~~text
ModuleType Version    Name                                ExportedCommands
---------- -------    ----                                ----------------
Binary     1.0.....   startexewithoutput                  Start-ExeWithOutput
~~~

[&uarr;](#TOC)

## Examples ##

~~~powershell
import-module  -name ".\PSStartExeWithOutput\StartExeWithOutput\bin\Release\StartExeWithOutput.dll
~~~

- Get help on cmdlets

~~~powershell
get-module StartExeWithOutput | select -expand ExportedCommands
$(get-module StartExeWithOutput).ExportedCommands.Keys
$(get-module StartExeWithOutput).ExportedCommands.Keys |% {get-help $_}
~~~

- Run `build.bat`

~~~powershell
$splat =@{
FilePath= 'cmd.exe';
ArgumentList= @('/c', "build.bat", $imageName);
WorkingDirectory = ".";
WhatIf = $WhatIfPreference
Verbose = $($VerbosePreference -eq 'Continue')
}
$("{0} {1} in {2}" -f $p.FilePath, $($p.ArgumentList -join " "), $p.WorkingDirectory) | write-verbose
$rc = StartExeWithOutput\Start-ExeWithOutput @splat -ExitCodeList @(0,1)
~~~