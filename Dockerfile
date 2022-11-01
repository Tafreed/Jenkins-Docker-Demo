FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
 WORKDIR /app
 
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["S3 API.csproj", "."]
RUN dotnet restore "S3 API.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "S3 API.csproj" -c Release -o /app/build
 
FROM build AS publish
RUN dotnet publish "S3 API.csproj" -c Release -o /app/publish
 
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "S3 API.dll"]