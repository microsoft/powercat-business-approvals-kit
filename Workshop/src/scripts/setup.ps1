#Must be the first statement in your script (not counting comments)
param([string] $user, [string] $reset)

. $PSScriptRoot\users.ps1
. $PSScriptRoot\security.ps1
. $PSScriptRoot\common.ps1
. $PSScriptRoot\test.ps1

Push-Location
$appPath = [System.IO.Path]::Combine((Get-AssetPath),"..")
Set-Location $appPath

$domain=(az account show --query "user.name" -o tsv).Split('@')[1]

if ( Test-Path $user ) {
    Write-Host "Found user file"
    $lines = Get-Content $user
    $total = $lines.Length
    $index = 0
    foreach($line in $lines) {
        if ( -not [System.String]::IsNullOrEmpty($line) ) {
            $index = $index + 1
            Write-Host "---------------------------------------------"
            Write-Host "$index of $total - $(Get-Date)"
            Write-Host "$line@$domain"
            Write-Host "---------------------------------------------"

            if ( $reset -eq "Y") {
                Reset-UserDevelopmentEnvironment "$line@$domain"
            } else {
                Invoke-SetupUserForWorkshop "$line@$domain"
            }
            
        }
    }
} else {
    Write-Host "Single user setup"
    if ( $reset -eq "Y") {
        Reset-UserDevelopmentEnvironment "$line@$domain"
    } else {
        Invoke-SetupUserForWorkshop "$line@$domain"
    }
}


