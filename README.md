# EBCEYS.Server-configuration

Сервис - хранилище конфигурационных файлов для контейнеров.

## Вводимые сущности

[Сущности - *Configs* & *Keys*](./docs/Entities.md)

## Поддерживаемые переменные окружения

[Поддерживаемые переменные окружения](./docs/EnvironmentVariables.md)

## Api контроллеры

[*ApiControllers*](./docs/WebApi.md)

## Конфигурационный файл

[*/app/appsettings.json*](./docs/appsettings.md)

## Пример *docker-compose.yaml* файла при автоматической передаче конфигов

По умолчанию сервис слушает 3000 порт. Можно переопределить используя, например, *ASPNETCORE_URLS*.

[*Docker* файл](./src/Dockerfile)

```yaml
services:
  ebceys.server-configuration:
    container_name: ebceys.server-configuration
    hostname: ebceys.server-configuration
    environment:
      - SERVICE_ENABLE_SWAGGER=true
      - SERVICE_DATABASE_PATH=/data/configuration.db 
      - DOCKER_CONNECTION_USE_DEFAULT=true
      - DOCKER_CONNECTION_URL=unix:///var/run/docker.sock
      - CONFIG_PROCESSOR_ENABLE=true
      - CONFIG_PROCESSOR_CONFIGS_PATH=/storage/configs
      - CONFIG_PROCESSOR_PROCESS_PERIOD=00:00:10
      - CONFIG_PROCESSOR_CONTAINER_LABEL_KEY=configuration.service.type.name
      - CONFIG_PROCESSOR_CONTAINER_CONFIG_PATH_LABEL_KEY=configuration.file.path
      - CONFIG_PROCESSOR_CONTAINER_LABEL_RESTART_AFTER=configuration.restartafter
      - KEYS_STORAGE_PATH=/storage/keys
      - KEYS_STORAGE_FORGET_OLD_KEYS=true
      - DBCLEANER_TIME_TO_STORE=00:00:10
    image: ebceys/server-configuration:1.0.0
    build:
      context: .
      dockerfile: EBCEYS.Server-configuration/Dockerfile
    volumes:
      - C:\\storage:/storage:rw
      - C:\\server-configuration\data:/data:rw
      - /var/run/docker.sock:/var/run/docker.sock
    ports:
      - "5005:3000"
  rabbitmq:
    container_name: rabbitmq
    hostname: rabbitmq
    environment:
      - RABBITMQ_CONFIG_FILE=/configs/rabbitmq.conf
    labels:
      configuration.service.type.name: "rabbitmq"
      configuration.file.path: "/configs"
      configuration.restartafter: "true"
    image: rabbitmq:4.0.5-management
    ports:
        - "5675:5672"
        - "15675:15672"
    depends_on:
      - ebceys.server-configuration
```