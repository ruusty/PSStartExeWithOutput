# RuustyPowerShellModules - StartExeWithOutput #

~~~
Project:        Ruusty Powershell Tools
Product:        RuustyPowerShellModules
Version:        1.0.0.0
Date:           2018-10-07 
Description:    Powershell binary cmdlet to run an executable.
~~~


<a name="TOC"></a>
# Table of Contents

- [Description](#Description)
- [Examples](#Examples)

<a name="Description"></a>
## Description [&uarr;](#TOC) ##


A *Powershell* binary cmdlet that

- Returns the exit code.
- Can optionally log Stdout to a file.
- Can optionally log Stderr to a file.
- Display the executables Stdout and Stderr to the console.
- When `-Verbose` display Stdout and Stderr (`start-transcript` will now log it.)
- Throw on invalid exit code


<a name="Examples"></a>
## Examples [&uarr;](#TOC) ##


~~~
import-module  -name ".\PSStartExeWithOutput\StartExeWithOutput\bin\Release\StartExeWithOutput.dll

~~~

- Get help on cmdlets
~~~
get-module StartExeWithOutput | select -expand ExportedCommands
$(get-module StartExeWithOutput).ExportedCommands.Keys
$(get-module StartExeWithOutput).ExportedCommands.Keys |% {get-help $_}
~~~

- Run `build.bat`
~~~
$p =@{
FilePath= 'cmd.exe';
ArgumentList= @('/c', "build.bat", $imageName);
WorkingDirectory = ".";
WhatIf = $WhatIfPreference 
Verbose = $($VerbosePreference -eq 'Continue') 
}
$("{0} {1} in {2}" -f $p.FilePath, $($p.ArgumentList -join " "), $p.WorkingDirectory) | write-verbose
$rc = StartExeWithOutput\Start-ExeWithOutput @p -ExitCodeList @(0,1)
~~~