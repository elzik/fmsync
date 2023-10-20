$repoRootPath = (Resolve-Path "$PSScriptRoot/../").Path
$repoRootPath = $repoRootPath.Substring(0,$repoRootPath.Length-1)
Write-Output "Repo root path:$repoRootPath"

dotnet publish $repoRootPath\src\Elzik.FmSync.Worker\Elzik.FmSync.Worker.csproj `
	--runtime win-x64 `
	--self-contained true `
	--configuration Release `
	-p:Platform=x64 `
	-p:PublishSingleFile=true

dotnet publish $repoRootPath\src\Elzik.FmSync.Console\Elzik.FmSync.Console.csproj `
	--runtime win-x64 `
	--self-contained true `
	--configuration Release `
	-p:Platform=x64 `
	-p:PublishSingleFile=true

dotnet build $repoRootPath\Installer\Elzik.FmSync.WindowsInstaller\Elzik.FmSync.WindowsInstaller.wixproj `
	--runtime win-x64 `
	--self-contained true `
	--configuration Release `
	-p:Platform=x64 `
	-p:PublishSingleFile=true
