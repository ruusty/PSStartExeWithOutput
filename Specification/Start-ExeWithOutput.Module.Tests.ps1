[CmdletBinding()]
param
(
)
#region initialisation
if ($PSCmdlet.MyInvocation.BoundParameters["Verbose"].IsPresent)
{
  $VerbosePreference = 'Continue'
}
$Verbose = @{ }
if ($env:COMPUTERNAME -like "flores")
{
  $Verbose.add("Verbose", $false)
}

<#
invoke-Pester -Script @{ Path = './start-ExeWithOutput.Module.Tests.ps1'; verbose = 'Continue' }

invoke-Pester -Script @{ Path = './start-ExeWithOutput.Module.Tests.ps1'; }
##>
Write-Host $('{0}==>{1}' -f '$Verbose', $Verbose)
Write-Host $('{0}==>{1}' -f '$VerbosePreference', $VerbosePreference)
$PSBoundParameters | Out-String | Write-Verbose

$here = Split-Path -Parent $MyInvocation.MyCommand.Path
#$sut = (Split-Path -Leaf $MyInvocation.MyCommand.Path) -replace '\.Tests\.', '.'
$name = [System.IO.Path]::GetFileNameWithoutExtension($MyInvocation.MyCommand.Path)
$ModuleName = (Split-Path -Leaf $MyInvocation.MyCommand.Path) -Replace ".Module.Tests.ps1"
$name = $ModuleName
Write-Verbose $('{0}:{1}' -f '$here', $here)
Write-Verbose $('{0}:{1}' -f '$sut', $sut)
Write-Verbose $('{0}:{1}' -f '$ModuleName', $ModuleName)

$ModulePath = Join-Path $here "..\StartExeWithOutput\bin\Release\StartExeWithOutput.dll"


$ModuleSetup= Join-Path $PSScriptRoot "Pester.Tests.Setup.ps1"
if (Test-Path $ModuleSetup) {. $ModuleSetup }


Import-Module $ModulePath -verbose
get-module startExeWithOutput
get-module startExeWithOutput | select -expand ExportedCommands
$(get-module startExeWithOutput).ExportedCommands.Keys | % { get-help $_ }



Describe "Start-ExeWithOutput" {
  Context 'Parameters' {
    It "Should support -whatif" {
      (!(((Get-Command -Name Start-ExeWithOutput).parameters["Whatif"]) -eq $null)) | Should be $true
    }
  }
  
  Context -Name 'Input'{
    It "Should fail for non-Existant Exe"  {
      { Start-ExeWithOutput "not-found.exe" } | Should  throw
    }

#  It "Should fail with no args for Ping.exe" {
#      { Start-ExeWithOutput "ping.exe" -ExitCodeList 0 } | Should  throw
#    }
    
    
    It "Should not fail with no args for Ping.exe with ExitCode 1" {
      { Start-ExeWithOutput "ping.exe" -ExitCodeList @(1) } | Should  not throw
    }
    
    It "Should succeed with single arg for Ping.exe" {
      {Start-ExeWithOutput "ping.exe" "127.0.0.1" @Verbose} | Should not throw
    }

    It "Should succeed with array of args for Ping.exe" {
      {Start-ExeWithOutput -filepath "ping.exe" @("127.0.0.1", "-n", "5") @Verbose} | Should not throw
    }
    

  }
  
  Context 'Execution' {
  It "Should not fail with array of args" {
    {
        Start-ExeWithOutput -filepath "whoami.exe" -ArgumentList @("/ALL","/FO","LIST") @Verbose
    } | Should not throw
  }

    It "should throw on non 0 exit " {
      {
        Start-ExeWithOutput -filepath "whoami.exe","/invalid-arg"
      }| Should throw
    }

    It "Should not fail with array of args" {
    {
        Start-ExeWithOutput -filepath "nslookup.exe" @("-querytype=mx", "holliday.id.au") @Verbose
      } | Should not throw
    }

    It "Should not fail with array of args" {
      { Start-ExeWithOutput -filepath "nslookup.exe" @("-querytype=ns", "holliday.id.au") @Verbose} | Should not throw
    }

  }

  Context 'Output'  {

  It "Should Contain arguments in output" {
    Start-ExeWithOutput -filepath "echo-args.exe" @("arg1","arg2","arg3") | Should be 0
  }

    It "Should Contain arguments in output #2" {
      Start-ExeWithOutput -filepath "echo-args.exe" @("arg1", "arg2", "arg3")     | Should be 0
    }

    It "Should success with array for dir"{
      { Start-ExeWithOutput -filepath "cmd.exe" @("/c", "dir", "/w") @Verbose } | Should not throw
    }
  }
}