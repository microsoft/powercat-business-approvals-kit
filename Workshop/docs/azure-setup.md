# Overview

This page includes a summary of how to use create a small Azure Linux based Virtual machine to assist you with the setup of development environments.

## Why is this important?

If you want to setup a class size of 20+ students each environment setup could take up to 20 minutes. By using an Azure Cloud based Virtual Machine you could automate a set of machines without the need to have an active PC connected to the Internet.

## Create the VM

```
exists=`az group exists --name ApprovalsKitSetup`
if [ $exists = 'false' ];
then
  az group create --name ApprovalsKitSetup --location westus
else
  echo 'Group already exists'
fi
match=`az vm list -d -o table --query "[?name=='ApprovalsKitSetupVM']"`
if [ "$match" = "" ];
then
  az vm create --resource-group ApprovalsKitSetup --name "ApprovalsKitSetupVM" --image "Canonical:0001-com-ubuntu-server-jammy:22_04-lts-gen2:latest" --size "Standard_B2s" --storage-sku Standard_LRS --os-disk-size-gb 63 --public-ip-sku Standard --admin-username accadmin --generate-ssh-keys --ssh-key-value ~/.ssh/azurevm.pub --storage-sku Standard_LRS
else
  echo 'VM Already exists'
fi
$running=`az vm list -d --query "[?powerState=='VM running' && name=='ApprovalsKitSetupVM']" -o table`
if [  "$running" = "" ];
then
    az vm start  --resource-group ApprovalsKitSetup --name "ApprovalsKitSetupVM"
fi
```

## Copy the install files to the VM

```bash
ip=`az vm list-ip-addresses --resource-group ApprovalsKitSetup --name ApprovalsKitSetupVM --query "[].virtualMachine.network.publicIpAddresses[0].ipAddress" --output tsv`
pathExist=`ssh -i ~/.ssh/azurevm "accadmin@$ip" [ -e "/home/accadmin/powercat-business-approvals-kit-main.zip" ]`
if [ "$pathExist" = "" ];
then
    scp -i ~/.ssh/azurevm powercat-business-approvals-kit-main.zip  "accadmin@$ip":/home/accadmin
fi
```

## Setup the machine

1. Create ssh session to get command line to your virtual machine

```bash
ip=`az vm list-ip-addresses --resource-group ApprovalsKitSetup --name ApprovalsKitSetupVM --query "[].virtualMachine.network.publicIpAddresses[0].ipAddress" --output tsv`
ssh -i ~/.ssh/azurevm "accadmin@$ip" -t -l bash
```

1. Remove all .NET packages

```bash
sudo apt remove 'dotnet*'
sudo apt remove 'aspnetcore*'
```

1. Delete PMC repository from APT, by deleting the repo .list file

```bash
sudo rm /etc/apt/sources.list.d/microsoft-prod.list
```

1. Run package update

```bash
sudo apt update
```

## Install Azure CLI

1. Install the Azure CLI using

```bash
curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash
```

## Restart you VM

1. Login to the virtual machine using ssh

```bash
ip=`az vm list-ip-addresses --resource-group ApprovalsKitSetup --name ApprovalsKitSetupVM --query "[].virtualMachine.network.publicIpAddresses[0].ipAddress" --output tsv`
ssh -i ~/.ssh/azurevm "accadmin@$ip" -t -l bash
```

1. Install required tools

```bash
sudo apt remove -y 'dotnet*' 'aspnet*' 'netstandard*'
sudo rm -f /etc/apt/sources.list.d/microsoft-prod.list
sudo sudo apt install -y dotnet-sdk-7.0 dotnet-runtime-6.0 unzip
dotnet tool install --global Microsoft.PowerApps.CLI.Tool
dotnet tool install --global PowerShell
dotnet tool install --global SecureStore.Client

cat << \EOF >> ~/.bash_profile
# Add .NET Core SDK tools
export PATH="$PATH:/home/accadmin/.dotnet/tools"
EOF
```

1. Unzip the installer

```bash
unzip powercat-business-approvals-kit-main.zip
```

1. Setup the install app and install dependencies

```pwsh
cd powercat-business-approvals-kit-main\Workshop\src\install
dotnet build
pwsh ./bin/Debug/net7.0/playwright.ps1 install
pwsh ./bin/Debug/net7.0/playwright.ps1 install-deps

cd ~/powercat-business-approvals-kit/Workshop
mkdir secure
cd secure
SecureStore create secrets.json --keyfile secret.key
SecureStore set DEMO_PASSWORD "SomePassword" --keyfile secret.key
SecureStore set CLIENT_ID "Azure Client id" --keyfile secret.key
SecureStore set CLIENT_SECRET "Azure Client secret" "--keyfile secret.key
SecureStore set ADMIN_APP_ID "Azure Admin Client id" --keyfile secret.key
SecureStore set ADMIN_APP_SECRET "Azure Admin Client secret value" --keyfile secret.key
```

1. Setup a user by first logging in as Global Administrator

```bash
pwsh
cd ~/powercat-business-approvals-kit/Workshop
. ./src/scripts/users.ps1
az login --allow-no-subscriptions
Invoke-SetupUserForWorkshop AdeleV@contoso.OnMicrosoft.com
```

## Post Setup

1. Login

```bash
ip=`az vm list-ip-addresses --resource-group ApprovalsKitSetup --name ApprovalsKitSetupVM --query "[].virtualMachine.network.publicIpAddresses[0].ipAddress" --output tsv`
ssh -i ~/.ssh/azurevm "accadmin@$ip" -t -l bash
```

1. Start PowerShell

```bash
pwsh
```

1. In the Power Shell

```pwsh
cd Workflow
. ./src/scripts/users.ps1
Invoke-SetupUserForWorkshop AdeleV@contoso.OnMicrosoft.com
```

## Automating Cloud Shell

Start the cloud shell

```bash
cat << \EOF >> ~/sh.sh
# Start SSH
ip=`az vm list-ip-addresses --resource-group ApprovalsKitSetup --name ApprovalsKitSetupVM --query "[].virtualMachine.network.publicIpAddresses[0].ipAddress" --output tsv`
ssh -i ~/.ssh/azurevm "accadmin@$ip" -t -l bash
EOF
chmod +x s.sh
./s.sh
```

## Automating Install

1. Create automated setup scripts

```bash
cat << \EOF >> ~/setup.ps1
param([string] $user)
Push-Location
Set-Location "/home/accadmin/powercat-business-approvals-kit/Workshop"
. ./src/scripts/users.ps1
Invoke-SetupUserForWorkshop "$user@contoso.OnMicrosoft.com"
Pop-Location
EOF
cat << \EOF >> ~/sh.sh
pwsh ./setup.ps1 $1
EOF
chmod +x s.sh
```

1. Start an install with no hang up

```bash
nohup ./s.sh ChristieC &
```
