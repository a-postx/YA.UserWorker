FROM mcr.microsoft.com/dotnet/aspnet:6.0-bullseye-slim AS base
# Disable the culture invariant mode which defaults to true in the base slim image
# (See https://github.com/dotnet/corefx/blob/8245ee1e8f6063ccc7a3a60cafe821d29e85b02f/Documentation/architecture/globalization-invari>
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV LC_ALL=en_US.UTF-8
ENV LANG=en_US.UTF-8

WORKDIR /app
EXPOSE 8080

# SDK image used to build and publish the application
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS sdk
# To use the debug build configuration pass --build-arg Configuration=Debug
ARG Configuration=Release
ENV DOTNET_CLI_TELEMETRY_OPTOUT=true \
    DOTNET_SKIP_FIRST_TIME_EXPERIENCE=true
WORKDIR /src
COPY ["src/YA.UserWorker.csproj", "src/"]
RUN dotnet restore "src/YA.UserWorker.csproj"

COPY . .
WORKDIR "/src/src"
RUN dotnet build "YA.UserWorker.csproj" -c $Configuration -o /app/build
# try to add /p:PublishTrimmed=true to reduce image size
RUN dotnet publish "YA.UserWorker.csproj" -c $Configuration --self-contained true -r linux-x64 -o /app/publish

FROM base AS runtime
WORKDIR /app
COPY --from=sdk /app/publish .
ENTRYPOINT ["dotnet", "YA.UserWorker.dll"]
