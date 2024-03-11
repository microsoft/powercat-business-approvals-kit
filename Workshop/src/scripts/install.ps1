winget install Microsoft.DotNet.SDK.8
winget install Microsoft.DotNet.Runtime.7
winget install Microsoft.DotNet.Runtime.6
winget install --id Microsoft.Powershell --source winget
winget install -e --id Microsoft.AzureCLI
dotnet tool install --global SecureStore.Client

Push-Location
cd $env:TEMP
Invoke-WebRequest -Uri https://aka.ms/PowerAppsCLI -OutFile powerapps-cli-1.0.msi
            
$files = (Get-ChildItem -Filter "powerapps-cli*.msi")

if ($files.Length -gt 0) {
    Write-Host "Installing PAC"
    Msiexec /i $files[0] /qb! /l*v install.log 
} else {
    Write-Error "Unable to install pac"
}
Pop-Location

try {
    pwsh --verion | Out-Null
} catch {
    Write-Host "Restart the PowerShell as pwsh not found"
    return
}

Push-Location

cd "$($env:TEMP)\powercat-business-approvals-kit"
Write-Host "Pull updates"
git pull
cd Workshop\src\install
Write-Host "Building install application"
dotnet build
pwsh ./bin/Debug/net7.0/playwright.ps1 install

try {
    pwsh --version
} catch {
    Write-Host "Restart the PowerShell as pwsh not found"
    return
}

try {
    pac help | Out-Null
    Write-Host "Power Platform CLI Installed"
} catch {
    Write-Host "Restart the PowerShell as pac not found"
    return
}

cd "$($env:TEMP)\powercat-business-approvals-kit\Workshop"

if ( -not (Test-Path -Path "secure") ) {
    New-Item -Name "secure" -ItemType Directory
}
cd secure

if ( -not (Test-Path -Path "secrets.key" -PathType Leaf) ) {
    SecureStore create secrets.json --keyfile secret.key
}

Pop-Location