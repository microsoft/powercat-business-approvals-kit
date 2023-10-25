#Must be the first statement in your script (not counting comments)
param([string] $user)

. $PSScriptRoot\users.ps1
. $PSScriptRoot\security.ps1
. $PSScriptRoot\common.ps1

Push-Location
$appPath = [System.IO.Path]::Combine($PSScriptRoot,"..","..")
Set-Location $appPath
$domain=(az account show --query "user.name" -o tsv).Split('@')[1]
Invoke-SetupUserForWorkshop "$user@$domain"