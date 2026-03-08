function Get-GitVersionToolVersion {
    [CmdletBinding()]
    param(
        [string]$RepoRoot = (Split-Path -Parent $PSScriptRoot)
    )

    $packagesPath = Join-Path $RepoRoot 'Directory.Packages.props'

    if (-not (Test-Path $packagesPath)) {
        throw "Directory.Packages.props not found at $packagesPath"
    }

    $xml = [xml](Get-Content $packagesPath)
    $pkg = $xml.SelectSingleNode("//PackageVersion[@Include='GitVersion.MsBuild']")
    if (-not $pkg) {
        throw "GitVersion.MsBuild package version not found in $packagesPath"
    }

    $version = $pkg.GetAttribute('Version')
    if (-not $version) {
        throw "Version attribute not found for GitVersion.MsBuild in $packagesPath"
    }

    return $version
}

Export-ModuleMember -Function Get-GitVersionToolVersion
