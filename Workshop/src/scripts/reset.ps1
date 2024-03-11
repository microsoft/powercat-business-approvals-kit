#Must be the first statement in your script (not counting comments)
param([string] $user)

. $PSScriptRoot\users.ps1

Reset-UserDevelopmentEnvironment $user