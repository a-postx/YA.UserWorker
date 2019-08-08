FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
WORKDIR /src
COPY ["src/YA.TenantWorker.csproj", "src/"]

RUN dotnet restore "src/YA.TenantWorker.csproj"
COPY . .
WORKDIR "/src/src"
RUN dotnet build "YA.TenantWorker.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "YA.TenantWorker.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "YA.TenantWorker.dll"]