#Must be the first statement in your script (not counting comments)
param([string] $user)

. $PSScriptRoot\users.ps1

Write-Host (Invoke-ValidateEnvironment $user | ConvertTo-Json)