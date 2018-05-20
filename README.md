# RuustyPowerShellModules - StartExeWithOutput #

~~~
Project:        Ruusty Powershell Tools
Product:        RuustyPowerShellModules
Version:        1.0.0.0
Date:           2018-05-20 
Description:    Powershell binary cmdlet to run an executable.
~~~


<a name="TOC"></a>
# Table of Contents

- [Description](#Description)

<a name="Description"></a>
## Description [&uarr;](#TOC) ##


Project to create a Powershell binary cmdlet that

- Returns the exit code.
- Can optionally log StdOut to a file.
- Can optionally log StdErr to a file.
- Display the executables Stdout and Stderr to the console.
- When `-Verbose` display Stdout and Stderr (`start-transcript` will now log it.)

~~~
import-module  -name ".\PSStartExeWithOutput\StartExeWithOutput\bin\Release\StartExeWithOutput.dll

~~~

~~~
get-module StartExeWithOutput | select -expand ExportedCommands
$(get-module StartExeWithOutput).ExportedCommands.Keys
$(get-module StartExeWithOutput).ExportedCommands.Keys |% {get-help $_}
~~~
