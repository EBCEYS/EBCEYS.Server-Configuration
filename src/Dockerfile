FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS publish
WORKDIR /src
COPY ./EBCEYS.Server-configuration/EBCEYS.Server-configuration.csproj ./

RUN dotnet restore "./EBCEYS.Server-configuration.csproj" --runtime linux-musl-x64
COPY ./EBCEYS.Server-configuration .
RUN dotnet publish "EBCEYS.Server-configuration.csproj" -c Release -o /app/publish \
  --no-restore \
  /p:UseAppHost=true \
  --runtime linux-musl-x64 \
  --self-contained true \
  /p:PublishSingleFile=true

FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-alpine

ARG HELATHCHECK_PORT=8080
ENV ASPNETCORE_URLS=http://*:3000;http://*:$HELATHCHECK_PORT
ENV HEALTHCHECKS_STARTING_PORT=$HELATHCHECK_PORT

RUN apk upgrade musl

WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT [ "./EBCEYS.Server-configuration" ]