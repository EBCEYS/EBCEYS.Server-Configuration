# �������������� ���������� ���������:

���������� ��������� ������� � ������������� ������ *[����������](https://github.com/EBCEYS/EBCEYS.ContainersEnvironment)*.

���� ��������� ������ � ������ *--help* ��� *-h*, �� � ������� ��������� ���������� � ���� �������������� ���������� ���������.

## 1. SERVICE_*

* *SERVICE_ENABLE_SWAGGER* - *true/false*. �� ��������� - *true*. ��������/��������� ������� *(http://\{host}:\{port}/swagger)*.
* *SERVICE_DATABASE_PATH* - ���������� ���� �� ���� ������. ���� ������ ��������� ���� *CONFIG_PROCESSOR_ENABLE=true*. �� ��������� - *configuration.db*.

## 2. DOCKER_CONNECTION_*

* *DOCKER_CONNECTION_USE_DEFAULT* - *true/false*. �� ��������� *true*. ������������� ����� � ������� � ����� �� ��������� (*unix:///var/run/docker.sock* - ��� linux).
* *DOCKER_CONNECTION_URL* - URL ��� ����������� � *docker*. �� ��������� *unix:///var/run/docker.sock*. ������������ ���� *DOCKER_CONNECTION_USE_DEFAULT=true*.
* *DOCKER_CONNECTION_DEFAULT_TIMEOUT* - ������� ����������� � *docker*. �� ��������� *00:00:10*.

## 3. CONFIG_PROCESSOR_*

* *CONFIG_PROCESSOR_ENABLE* - *true/false*. �� ��������� *true*. �������� �� ���������/���������� �������������� �������� �������� � ����������.
* *CONFIG_PROCESSOR_CONFIGS_PATH* - ���������� ���� �� �������� �����������. �� ��������� */storage/configs/*.
* *CONFIG_PROCESSOR_PROCESS_PERIOD* - ������ �������� ����������� �� ������������� �������� ������������. �� ��������� *00:00:05*.
* *CONFIG_PROCESSOR_CONTAINER_LABEL_KEY* - ���� ������ ����������, � ������� ����������� ��� ���� ����������. �� ��������� *configuration.service.type.name*.
* *CONFIG_PROCESSOR_CONTAINER_CONFIG_PATH_LABEL_KEY* - ���� ������ ����������, � ������� ����������� ���� �� ��������� ��������. �� ��������� *configuration.file.path*.
* *CONFIG_PROCESSOR_CONTAINER_LABEL_RESTART_AFTER* - ���� ������ ����������, � ������� ����������� ������������� �������� ���������� ����� �������� �������. �� ��������� *configuration.restartafter*.

## 4. KEYS_STORAGE_*

* *KEYS_STORAGE_PATH* - ���������� ���� �� ��������� ������. �� ��������� */storage/keys*.
* *KEYS_STORAGE_FILE_CHECK_PERIOD* - ������ �������� ���������� ������ ������. �� ��������� *00:00:05*.
* *KEYS_STORAGE_FORGET_OLD_KEYS* - *true/false*. �������� ������ ����� ��� ���. �� ������ ���� ���-�� ������ ���� �� ���������. ��� ������������ �������, ���� ���������. �� ��������� *false*.

## 5. DBCLEANER_*

* *DBCLEANER_TIME_TO_STORE* - ����� �������� ���������� � ��������� ����������� � ���� ������. �� ��������� *00:00:00* (�� ���� ��������� �����).

## ������ ������������ � *docker-compose.yaml*

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