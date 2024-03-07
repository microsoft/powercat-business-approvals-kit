function Invoke-InstallChocoApp {
    param (
        [Parameter(Mandatory)] $ApptoInstall,
        $Version
    )

    $installed = (& "$($env:ChocolateyInstall)\bin\choco" list $ApptoInstall)
    $newInstall = $null -ne ($installed | ? { $_.IndexOf( "0 packages installed") -ge 0 }) 
    if ($newInstall -or ( -not [System.String]::IsNullOrEmpty($Version) ) ) {
        Write-Host $ApptoInstall installing
        # We are not running as an administrator, so relaunch as administrator

        # Create a new process object that starts PowerShell
        $newProcess = New-Object System.Diagnostics.ProcessStartInfo "$($env:ChocolateyInstall)\bin\choco";

        # Specify the current script path and name as a parameter with added scope and support for scripts with spaces in it's path
        $newProcess.Arguments = "install -y $ApptoInstall"

        if ( -not [System.String]::IsNullOrEmpty($Version) ) {
            $newProcess.Arguments += " --version " + $Version
        }

        $newProcess.UseShellExecute = $true

        # Indicate that the process should be elevated
        $newProcess.Verb = "runas";

        # Start the new process
        $process = [System.Diagnostics.Process]::Start($newProcess);
        $process.WaitForExit();

        # Exit from the current, unelevated, process
        Exit;
    } else {
        Write-Host "Installed"
    }
}

Invoke-InstallChocoApp "dotnet-6.0-sdk"
Invoke-InstallChocoApp "dotnet"

try {
    dotnet | Out-Null
} catch {
    Write-Host "Restart the PowerShell"
    return
}

winget install Microsoft.DotNet.SDK.8
winget install -e --id Microsoft.AzureCLI
dotnet tool install --global Microsoft.PowerApps.CLI.Tool
dotnet tool install --global SecureStore.Client

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