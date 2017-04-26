
get-module Ruusty.Powershell | select -expand ExportedCommands
$(get-module Ruusty.Powershell).ExportedCommands.Keys
$(get-module Ruusty.Powershell).ExportedCommands.Keys |% {get-help $_}
