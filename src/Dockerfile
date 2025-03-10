# См. статью по ссылке https://aka.ms/customizecontainer, чтобы узнать как настроить контейнер отладки и как Visual Studio использует этот Dockerfile для создания образов для ускорения отладки.

# Этот этап используется при запуске из VS в быстром режиме (по умолчанию для конфигурации отладки)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
ARG HELATHCHECK_PORT=8080
ENV ASPNETCORE_URLS=http://*:3000;http://*:$HELATHCHECK_PORT
ENV HEALTHCHECKS_STARTING_PORT=$HELATHCHECK_PORT


# Этот этап используется для сборки проекта службы
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["./EBCEYS.Server-configuration/EBCEYS.Server-configuration.csproj", "./EBCEYS.Server-configuration/"]
RUN dotnet restore "./EBCEYS.Server-configuration/EBCEYS.Server-configuration.csproj"
COPY . .
WORKDIR "./EBCEYS.Server-configuration"
RUN dotnet build "./EBCEYS.Server-configuration.csproj" -c $BUILD_CONFIGURATION -o /app/build --self-contained true -r linux-x64

# Этот этап используется для публикации проекта службы, который будет скопирован на последний этап
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./EBCEYS.Server-configuration.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=true --self-contained true -r linux-x64

# Этот этап используется в рабочей среде или при запуске из VS в обычном режиме (по умолчанию, когда конфигурация отладки не используется)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN echo $(ls /app)
ENTRYPOINT ["/app/EBCEYS.Server-configuration"]