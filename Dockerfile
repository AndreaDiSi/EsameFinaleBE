FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/AutoConfig.Core/AutoConfig.Core.csproj", "src/AutoConfig.Core/"]
COPY ["src/AutoConfig.Infrastructure/AutoConfig.Infrastructure.csproj", "src/AutoConfig.Infrastructure/"]
COPY ["src/AutoConfig.Api/AutoConfig.Api.csproj", "src/AutoConfig.Api/"]
RUN dotnet restore "src/AutoConfig.Api/AutoConfig.Api.csproj"
COPY . .
RUN dotnet publish "src/AutoConfig.Api/AutoConfig.Api.csproj" -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AutoConfig.Api.dll"]
