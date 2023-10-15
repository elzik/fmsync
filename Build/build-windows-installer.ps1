dotnet publish $PSScriptRoot\..\src\Elzik.FmSync.Worker\Elzik.FmSync.Worker.csproj `
	--runtime win-x64 `
	--self-contained true `
	--configuration Release `
	-p:Platform=x64 `
	-p:PublishSingleFile=true

dotnet publish $PSScriptRoot\..\src\Elzik.FmSync.Console\Elzik.FmSync.Console.csproj `
	--runtime win-x64 `
	--self-contained true `
	--configuration Release `
	-p:Platform=x64 `
	-p:PublishSingleFile=true

dotnet build $PSScriptRoot\..\Installer\Elzik.FmSync.WindowsInstaller\Elzik.FmSync.WindowsInstaller.wixproj `
	--runtime win-x64 `
	--self-contained true `
	--configuration Release `
	-p:Platform=x64 `
	-p:PublishSingleFile=true
