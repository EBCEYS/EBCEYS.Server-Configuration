# Поддерживаемые переменные окружения:

Переменные окружения берутся с использованем данной *[библиотеки](https://github.com/EBCEYS/EBCEYS.ContainersEnvironment)*.

Если запустить сервис с ключем *--help* или *-h*, то в консоль выводится информация о всех поддерживаемых переменных окружения.

## 1. SERVICE_*

* *SERVICE_ENABLE_SWAGGER* - *true/false*. По умолчанию - *true*. Включает/выключает сваггер *(http://\{host}:\{port}/swagger)*.
* *SERVICE_DATABASE_PATH* - абсолютный путь до базы данных. База данных создается если *CONFIG_PROCESSOR_ENABLE=true*. По умолчанию - *configuration.db*.

## 2. DOCKER_CONNECTION_*

* *DOCKER_CONNECTION_USE_DEFAULT* - *true/false*. По умолчанию *true*. Устанавливать связь с докером с путем по умолчанию (*unix:///var/run/docker.sock* - для linux).
* *DOCKER_CONNECTION_URL* - URL для подключения к *docker*. По умолчанию *unix:///var/run/docker.sock*. Игнорируется если *DOCKER_CONNECTION_USE_DEFAULT=true*.
* *DOCKER_CONNECTION_DEFAULT_TIMEOUT* - таймаут подключения к *docker*. По умолчанию *00:00:10*.

## 3. CONFIG_PROCESSOR_*

* *CONFIG_PROCESSOR_ENABLE* - *true/false*. По умолчанию *true*. Отвечает за включение/выключение автоматической отправки конфигов в контейнеры.
* *CONFIG_PROCESSOR_CONFIGS_PATH* - абсолютный путь до конфигов контейнеров. По умолчанию */storage/configs/*.
* *CONFIG_PROCESSOR_PROCESS_PERIOD* - период проверки контейнеров на необходимость отправки конфигурации. По умолчанию *00:00:05*.
* *CONFIG_PROCESSOR_CONTAINER_LABEL_KEY* - ключ лейбла контейнера, в котором указывается имя типа контейнера. По умолчанию *configuration.service.type.name*.
* *CONFIG_PROCESSOR_CONTAINER_CONFIG_PATH_LABEL_KEY* - ключ лейбла контейнера, в котором указывается путь до хранилища конфигов. По умолчанию *configuration.file.path*.
* *CONFIG_PROCESSOR_CONTAINER_LABEL_RESTART_AFTER* - ключ лейбла контейнера, в котором указывается необходимость рестарта контейнера после отправки конфига. По умолчанию *configuration.restartafter*.

## 4. KEYS_STORAGE_*

* *KEYS_STORAGE_PATH* - абсолютный путь до хранилища ключей. По умолчанию */storage/keys*.
* *KEYS_STORAGE_FILE_CHECK_PERIOD* - период проверки обновления файлов ключей. По умолчанию *00:00:05*.
* *KEYS_STORAGE_FORGET_OLD_KEYS* - *true/false*. Забиваем старые ключи или нет. На случай если кто-то удалил ключ из хранилища. При перезагрузке сервиса, ключ забудется. По умолчанию *false*.

## 5. DBCLEANER_*

* *DBCLEANER_TIME_TO_STORE* - время хранения информации о удаленных контейнерах в базе данных. По умолчанию *00:00:00* (то есть удаляются сразу).

## Пример конфигурации в *docker-compose.yaml*

```yaml
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
    image: ${DOCKER_REGISTRY-}ebceysserverconfiguration
    build:
      context: .
      dockerfile: EBCEYS.Server-configuration/Dockerfile
    volumes:
      - C:\\storage:/storage:rw
      - C:\\server-configuration\data:/data:rw
      - /var/run/docker.sock:/var/run/docker.sock
    ports:
      - "5005:3000"
```