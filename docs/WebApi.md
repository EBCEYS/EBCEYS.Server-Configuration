# WebApi

## *ConfigurationController* */api/configuration/*

### *GET* */files/info?containerTypeName=\{container\}&containerSavePath=\{absSavePath\}*

Метод возвращает информацию о конфигурационных файлов для данного типа контейнера.

[Класс из библиотеки](https://github.com/EBCEYS/EBCEYS.ContainersEnvironment/blob/master/EBCEYS.ContainersEnvironment/Configuration/Models/ConfigurationFileInfo.cs)

```json
[
  {
    "serverFileFullPath": "string",
    "lastWriteUTC": "2025-03-06T18:52:04.140Z",
    "containerTypeName": "string",
    "fileSaveFullPath": "string"
  }
]
```
Где:
* *serverFileFullPath* - путь до файла на сервере, по которому можно этот файл запросить.
* *lastWriteUTC* - время последнего изменения файла.
* *containerTypeName* - имя типа контейнера (эквивалентно *containerTypeName* из query).
* *fileSaveFullPath* - полный путь для сохранения файла в контейнере.

### *GET* */files/\{filePath\}*

Метод отдает файл конфигурации. *filePath* должен быть абсолютным и содержать директорию хранения конфигов.

Предполагается, что будет браться из *serverFileFullPath* из предыдущего запроса.

### *GET* */archive/tar?typeName=\{container\}*

Возвращается tar архив с конфигами.

*typeName* - опциональный параметр. Если указан, то возвращается конфиги указанного контейнера, если нет, то все конфиги.

### *PATCH* */archive/tar?removeOldFiles=\{true/false\}*

Загружает новые конфиги на сервер. Если *removeOldFiles=true*, то удаляет все старые конфиги и кладет новые. Иначе перезаписывает существующие.

## *KeysController* */api/keys*

### *PATCH* */archive/tar?removeOldFiles=\{true/false\}*

Загружает новые ключи на сервер. Если *removeOldFiles=true*, то удаляет все старые ключи (только файлы, в памяти остаются если *KEYS_STORAGE_FORGET_OLD_KEYS=true*). Иначе перезаписывает существующие.

### *GET* */archive/tar*

Возвращает tar архив с ключами.

## *DockerApiController*

Не собирается в *release*. Так что игнорируйте его.