# PSStartExeWithOutput #

<pre style="font-size: .75em;"><code>
Project:        PSStartExeWithOutput
Product:        PSStartExeWithOutput
Version:        0.0.0.0
Date:           2018-05-21 
Description:    Specification for the PSStartExeWithOutput and used by Pester Tests.

CHED Services
</code></pre>


<a name="TOC"></a>
# Table of Contents

- [Description](#Description)

<a name="Description"></a>
## Description [^](#TOC) ##

~~~
import-module "..\StartExeWithOutput\StartExeWithOutput\bin\Release\StartExeWithOutput.dll"

get-module StartExeWithOutput | select -expand ExportedCommands
$(get-module StartExeWithOutput).ExportedCommands.Keys
$(get-module StartExeWithOutput).ExportedCommands.Keys |% {get-help $_}

~~~

~~~
invoke-Pester -Script @{ Path = './start-ExeWithOutput.Module.Tests.ps1'; verbose = 'Continue' }
~~~


## Notes ##

~~~
start powershell.exe -noExit -command '& {import-module '..\StartExeWithOutput\StartExeWithOutput\bin\Release\StartExeWithOutput.dll' ;Start-ExeWithOutput -filepath whoami.exe -argumentlist @('/ALL','/FO','LIST') -verbose}"

start powershell.exe -noExit -command "& {import-module '..\StartExeWithOutput\StartExeWithOutput\bin\Release\StartExeWithOutput.dll' ;Start-ExeWithOutput -filepath 'ping.exe' @('8.8.8.8', '-n', '5')}"

invoke-Pester -Script @{ Path = './start-ExeWithOutput.Module.Tests.ps1'; verbose = 'Continue' }

~~~

~~~

NAME
    Start-ExeWithOutput
    
SYNTAX
    Start-ExeWithOutput [-FilePath] <string> [[-ArgumentList] <string[]>] [-WorkingDirectory <string>] [-LogPathStdout <string>] [-LogPathStderr <string>] 
    [-ExitCodeList <int[]>] [-WhatIf] [-Confirm]  [<CommonParameters>]
    

ALIASES
    None
    

REMARKS
    None



~~~
