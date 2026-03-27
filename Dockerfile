FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY src/MediaLibrary/MediaLibrary.csproj src/MediaLibrary/
RUN dotnet restore src/MediaLibrary/MediaLibrary.csproj

COPY src/MediaLibrary/ src/MediaLibrary/
RUN dotnet publish src/MediaLibrary/MediaLibrary.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends ffmpeg && rm -rf /var/lib/apt/lists/*

COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "MediaLibrary.dll"]
