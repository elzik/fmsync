dotnet tool update --global GitVersion.Tool
$semVer = (dotnet-gitversion | ConvertFrom-Json).SemVer
$tag = "v$semVer"
Write-Output "tag=$tag" >> $GITHUB_OUTPUT
Write-Host $tag