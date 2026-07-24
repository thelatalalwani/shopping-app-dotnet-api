# ============================================
# Export-Code.ps1
# Exports all source code files into a single
# api-source.txt file.
# ============================================

$ErrorActionPreference = "Stop"

# -------------------------------------------------------
# CHANGE THESE PATHS ONLY
# -------------------------------------------------------

# Root folder of your .NET project
$ProjectPath = "C:\dev\shopping-app-dotnet-api"

# Folder where output file should be created
$OutputFolder = "C:\dev\shopping-app-dotnet-api\Scripts\output"

# Output file name
$OutputFile = Join-Path $OutputFolder "api-source.txt"

# -------------------------------------------------------
# File extensions to include
# -------------------------------------------------------

$IncludeExtensions = @(
    ".cs",
    ".csproj",
    ".sln",
    ".slnx",
    ".razor",
    ".cshtml",
    ".json",
    ".xml",
    ".config",
    ".sql",
    ".http",
    ".props",
    ".targets",
    ".yml",
    ".yaml",
    ".js",
    ".jsx",
    ".ts",
    ".tsx",
    ".html",
    ".css",
    ".scss",
    ".md"
)

# -------------------------------------------------------
# Ignore folders
# -------------------------------------------------------

$IgnoreFolders = @(
    "bin",
    "obj",
    ".git",
    ".github",
    ".vs",
    ".vscode",
    ".idea",
    "node_modules",
    "packages",
    "TestResults",
    "coverage",
    "coverage-report",
    "publish",
    "dist",
    "build"
)

# -------------------------------------------------------
# Ignore files
# -------------------------------------------------------

$IgnoreFiles = @(
    "api-source.txt",
    "package-lock.json",
    "yarn.lock",
    "pnpm-lock.yaml"
)

# -------------------------------------------------------
# Ignore patterns
# -------------------------------------------------------

$IgnorePatterns = @(
    "*.dll",
    "*.exe",
    "*.pdb",
    "*.cache",
    "*.log",
    "*.tmp",
    "*.user",
    "*.suo",
    "*.min.js",
    "*.min.css"
)

# -------------------------------------------------------
# Validate paths
# -------------------------------------------------------

if (!(Test-Path $ProjectPath))
{
    Write-Host ""
    Write-Host "Project folder not found:"
    Write-Host $ProjectPath
    exit
}

# Resolve full path
$ProjectPath = (Resolve-Path $ProjectPath).Path.TrimEnd("\")

# Create output folder if it doesn't exist
if (!(Test-Path $OutputFolder))
{
    New-Item -ItemType Directory -Path $OutputFolder | Out-Null
}

# Delete previous output file
if (Test-Path $OutputFile)
{
    Remove-Item $OutputFile -Force
}

# Create empty output file
"" | Out-File $OutputFile -Encoding utf8

# -------------------------------------------------------
# Read source files
# -------------------------------------------------------

$Files = Get-ChildItem -Path $ProjectPath -Recurse -File |
Where-Object {

    # Ignore generated output file
    if ($_.FullName -eq $OutputFile)
    {
        return $false
    }

    # Ignore unsupported extensions
    if ($IncludeExtensions -notcontains $_.Extension.ToLower())
    {
        return $false
    }

    # Ignore specific files
    if ($IgnoreFiles -contains $_.Name)
    {
        return $false
    }

    # Ignore file patterns
    foreach ($pattern in $IgnorePatterns)
    {
        if ($_.Name -like $pattern)
        {
            return $false
        }
    }

    # Ignore folders
    foreach ($folder in $IgnoreFolders)
    {
        if ($_.FullName -match "\\$([regex]::Escape($folder))(\\|$)")
        {
            return $false
        }
    }

    return $true
} |
Sort-Object FullName

# -------------------------------------------------------
# Export files
# -------------------------------------------------------

$count = 0

foreach ($file in $Files)
{
    $relativePath = $file.FullName.Substring($ProjectPath.Length).TrimStart("\")

    Add-Content $OutputFile ""
    Add-Content $OutputFile ("=" * 120)
    Add-Content $OutputFile ("FILE : $relativePath")
    Add-Content $OutputFile ("=" * 120)
    Add-Content $OutputFile ""

    Get-Content $file.FullName | Add-Content $OutputFile

    Add-Content $OutputFile ""
    Add-Content $OutputFile ""

    $count++
}

# -------------------------------------------------------
# Summary
# -------------------------------------------------------

Write-Host ""
Write-Host "==========================================" -ForegroundColor Green
Write-Host "Export completed successfully." -ForegroundColor Green
Write-Host "Files exported : $count" -ForegroundColor Green
Write-Host "Output file    : $OutputFile" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Green