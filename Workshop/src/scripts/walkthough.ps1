<#
.SYNOPSIS  
    Generate audio talk track for the recorded demo using Azure Speech synthensis command line
.DESCRIPTION  
    This script provides a sample how to setup workshop environment
.NOTES  
    File Name  : walkthrough.ps1  
    Author     : Grant Archibald

Copyright (c) Microsoft Corporation. All rights reserved.
Licensed under the MIT License.
#>

param (
    [Parameter(mandatory=$true)]
    [string]$section
)

Import-Module Microsoft.PowerShell.Utility
Import-Module PowerHTML

$md = ConvertFrom-Markdown $PSScriptRoot\walkthrough.md

$result = $md.Html | ConvertFrom-HTML

foreach ($node in $result.SelectNodes("//h2"))
{
    $file = $node.InnerText.ToLower().Replace(" ", "-").Trim() + ".wav"

    if ( $file.StartsWith($section)) {
        $next = $node.NextSibling
        while ( [System.String]::IsNullOrEmpty($next.InnerText.Trim()) ) {
            $next = $next.NextSibling
        }
        $text = $next.InnerText

        $audioPath = [System.IO.Path]::Combine($PSScriptRoot,"audio")

        if ( -not (Test-Path $audioPath) ) {
            New-Item -ItemType Directory -Force -Path $audioPath
        }

        $audioFile = [System.IO.Path]::Combine($audioPath,$file)

        # Read more https://learn.microsoft.com/azure/ai-services/speech-service/spx-basics
        spx synthesize --text "$text" --audio output "$audioFile" --voice en-US-AndrewNeural

        # Remove log file
        Remove-Item log*.log
    }
}