dotnet tool update --global GitVersion.Tool
dotnet-gitversion
$semVer = (dotnet-gitversion | ConvertFrom-Json).SemVer
$tag = "v$semVer"
Write-Output "tag=$tag" >> $GITHUB_OUTPUT
Write-Output $tag