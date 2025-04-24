FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/NeoServiceLayer.Api/NeoServiceLayer.Api.csproj", "NeoServiceLayer.Api/"]
COPY ["src/NeoServiceLayer.Core/NeoServiceLayer.Core.csproj", "NeoServiceLayer.Core/"]
COPY ["src/NeoServiceLayer.Services/NeoServiceLayer.Services.csproj", "NeoServiceLayer.Services/"]
COPY ["src/NeoServiceLayer.Enclave/NeoServiceLayer.Enclave.csproj", "NeoServiceLayer.Enclave/"]
COPY ["src/NeoServiceLayer.Common/NeoServiceLayer.Common.csproj", "NeoServiceLayer.Common/"]
RUN dotnet restore "NeoServiceLayer.Api/NeoServiceLayer.Api.csproj"
COPY src/ .
WORKDIR "/src/NeoServiceLayer.Api"
RUN dotnet build "NeoServiceLayer.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NeoServiceLayer.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NeoServiceLayer.Api.dll"]
