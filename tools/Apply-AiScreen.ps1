[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$ProjectPath,

    [Parameter(Mandatory = $true)]
    [string]$ScreenName,

    [Parameter(Mandatory = $true)]
    [string]$AiOutputFile,

    [string]$CliPath = ".\winccml-import-export.Cli\bin\Debug\winccml-import-export.Cli.exe",

    [switch]$Commit,

    [switch]$Push
)

$ErrorActionPreference = "Stop"

function Save-FileSection {
    param(
        [hashtable]$Sections,
        [string]$Name,
        [System.Collections.ArrayList]$Content
    )

    if ([string]::IsNullOrWhiteSpace($Name)) {
        throw "AI output contains an empty FILE name."
    }

    if ($Sections.ContainsKey($Name)) {
        throw "Duplicate FILE section: $Name"
    }

    $Sections[$Name] = ($Content -join [Environment]::NewLine)
}

function Invoke-GitCommand {
    param(
        [string[]]$Arguments
    )

    & git @Arguments

    if ($LASTEXITCODE -ne 0) {
        throw "Git command failed: git $($Arguments -join ' ')"
    }
}

$RepoRoot = (& git rev-parse --show-toplevel).Trim()

if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($RepoRoot)) {
    throw "The current directory is not inside a Git repository."
}

$RepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)
Set-Location $RepoRoot

$ProjectPath = [System.IO.Path]::GetFullPath($ProjectPath)
$AiOutputFile = [System.IO.Path]::GetFullPath($AiOutputFile)
$CliPath = [System.IO.Path]::GetFullPath($CliPath)

if (-not (Test-Path -LiteralPath $ProjectPath -PathType Leaf)) {
    throw "TIA Portal project file was not found: $ProjectPath"
}

if (-not (Test-Path -LiteralPath $AiOutputFile -PathType Leaf)) {
    throw "AI output file was not found: $AiOutputFile"
}

if (-not (Test-Path -LiteralPath $CliPath -PathType Leaf)) {
    throw "CLI executable was not found: $CliPath"
}

if ([string]::IsNullOrWhiteSpace($ScreenName)) {
    throw "Screen name cannot be empty."
}

$invalidCharacters = [System.IO.Path]::GetInvalidFileNameChars()

foreach ($character in $invalidCharacters) {
    if ($ScreenName.Contains($character)) {
        throw "Screen name contains an invalid file-name character: $character"
    }
}

$expectedFiles = @(
    "$ScreenName.screen.hmi.yml",
    "$ScreenName.screen.FontLibrary.hmi.yml",
    "$ScreenName.screen.TextLibrary.hmi.yml",
    "$ScreenName.screen.VirtualEventModule.hmi.js",
    "$ScreenName.screen.VirtualTriggerModule.hmi.js"
)

$sections = @{}
$currentFileName = $null
$currentContent = New-Object System.Collections.ArrayList

foreach ($line in Get-Content -LiteralPath $AiOutputFile) {
    if ($line -match '^FILE:\s*(.+?)\s*$') {
        if ($null -ne $currentFileName) {
            Save-FileSection $sections $currentFileName $currentContent
        }

        $currentFileName = $matches[1].Trim()
        $currentContent = New-Object System.Collections.ArrayList
        continue
    }

    if ($line -match '^END_FILE\s*$') {
        if ($null -eq $currentFileName) {
            throw "Found END_FILE without a matching FILE marker."
        }

        Save-FileSection $sections $currentFileName $currentContent
        $currentFileName = $null
        $currentContent = New-Object System.Collections.ArrayList
        continue
    }

    if ($null -ne $currentFileName) {
        if ($line -notmatch '^```') {
            [void]$currentContent.Add($line)
        }
    }
}

if ($null -ne $currentFileName) {
    Save-FileSection $sections $currentFileName $currentContent
}

foreach ($expectedFile in $expectedFiles) {
    if (-not $sections.ContainsKey($expectedFile)) {
        throw "AI output is missing required file: $expectedFile"
    }
}

$generatedDirectory = Join-Path $RepoRoot "ai\generated\$ScreenName"
New-Item -ItemType Directory -Force $generatedDirectory | Out-Null

$utf8WithoutBom = New-Object System.Text.UTF8Encoding($false)

foreach ($expectedFile in $expectedFiles) {
    $targetPath = Join-Path $generatedDirectory $expectedFile
    [System.IO.File]::WriteAllText(
        $targetPath,
        $sections[$expectedFile],
        $utf8WithoutBom)

    Write-Host "Generated: $targetPath"
}

$mainFile = Join-Path $generatedDirectory "$ScreenName.screen.hmi.yml"
$mainContent = Get-Content -LiteralPath $mainFile -Raw

if ($mainContent -notmatch '(?m)^#Version:\s*2\.0\s*$') {
    throw "Main HMI YAML is missing '#Version: 2.0'."
}

if ($mainContent -notmatch '(?m)^#DirectExport\s*$') {
    throw "Main HMI YAML is missing '#DirectExport'."
}

if ($mainContent -notmatch '(?m)^Screens:\s*$') {
    throw "Main HMI YAML is missing the Screens root node."
}

foreach ($expectedFile in $expectedFiles) {
    if ($expectedFile -ne "$ScreenName.screen.hmi.yml") {
        if ($mainContent -notmatch [regex]::Escape($expectedFile)) {
            Write-Warning "Main YAML does not explicitly reference $expectedFile."
        }
    }
}

Write-Host "Resource package validation completed."

Write-Host "Importing screen into TIA Portal project..."
& $CliPath "import-screen" $ProjectPath $mainFile

if ($LASTEXITCODE -ne 0) {
    throw "TIA Portal screen import failed. Git commit was not executed."
}

Write-Host "TIA Portal screen import completed successfully."

if ($Push -and -not $Commit) {
    throw "The -Push switch requires the -Commit switch."
}

if ($Commit) {
    $relativeGeneratedDirectory = "ai\generated\$ScreenName"

    Invoke-GitCommand @("add", "--", $relativeGeneratedDirectory)

    $commitMessage = "Add AI-generated WinCC Unified screen $ScreenName"
    Invoke-GitCommand @("commit", "-m", $commitMessage)

    Write-Host "Git commit completed."

    if ($Push) {
        Invoke-GitCommand @("push")
        Write-Host "Git push completed."
    }
}