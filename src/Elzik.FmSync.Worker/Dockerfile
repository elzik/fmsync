#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Elzik.FmSync.Worker/Elzik.FmSync.Worker.csproj", "Elzik.FmSync.Worker/"]
RUN dotnet restore "Elzik.FmSync.Worker/Elzik.FmSync.Worker.csproj"
COPY . .
WORKDIR "/src/Elzik.FmSync.Worker"
RUN dotnet build "Elzik.FmSync.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Elzik.FmSync.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Elzik.FmSync.Worker.dll"]