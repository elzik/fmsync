dotnet tool update --global GitVersion.Tool  --version 6.*

$semVer = (dotnet-gitversion | ConvertFrom-Json).SemVer
$tag = "v$semVer"

git tag $tag -f
git push --tags

Write-Output "Tagged with $tag"