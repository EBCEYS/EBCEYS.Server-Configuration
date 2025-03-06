# Вводимые сущности:

## 1. Configs

Хранятся в директории, которая указывается в *CONFIG_PROCESSOR_CONFIGS_PATH*.

Пример пути для хранения конфигурации контейнера: */storage/configs/rabbitmq/rabbitmq.conf*, где:
* */storage/configs/* - абсолютный путь до директории из *CONFIG_PROCESSOR_CONFIGS_PATH*
* */rabbitmq/* - имя типа контейнера
* *rabbitmq.conf* - файл конфигурации

Если *CONFIG_PROCESSOR_ENABLE=true*, то конфиги будут подкладываться (сохраняя структуру директории) автоматически в контейнер со следующими лейблами:
* *CONFIG_PROCESSOR_CONTAINER_LABEL_KEY* - лейбл, который должен быть эквивалентен имени типа контейнера (*rabbitmq*)
* *CONFIG_PROCESSOR_CONTAINER_CONFIG_PATH_LABEL_KEY* - лейбл, в котором указывается путь до хранилища конфигурации
* *CONFIG_PROCESSOR_CONTAINER_LABEL_RESTART_AFTER* - **ОПЦИОНАЛЬНЫЙ** лейбл, который указывает нужно ли перезагружать контейнер после того как подложили новый конфиг

Если *CONFIG_PROCESSOR_ENABLE=false*, то конфиги подкладываться автоматически не будут (рекомендуемый способ).

В этом случае их необходимо запрашивать по *WebApi*. Метод *configuration/files/info*, где в параметрах указывается:
* containerTypeName - имя типа контейнера (*rabbitmq*)
* containerSavePath - путь, куда сохранять конфиги в контейнере

**ВАЖНО!!!** В данном случае конфиги автоматически не подложатся. Их необходимо по файлу получать по запросу *configuration/files/{filePath}*, где *filePath* - абсолютный путь до конфиг файла на сервере.

## 2. Keys

Хранятся в директории, которая указывается в *KEYS_STORAGE_PATH*.

Пример пути для хранения ключей: */storage/keys/rabbitmq.key*.

Файл **.key* представляет из себя *json* документ.

Пример:
```json
{
  "username": "admin",
  "password": "admin"
}
```

В таком случае, в системе ключи будут иметь следующий вид:
```
<<rabbitmq.username>> = admin
<<rabbitmq.password>> = admin
```
Ключи будут автоматически подкладываться в конфигурационный файл при его запросе.

Например:

Имеем следующий конфигурационный файл */storage/configs/rabbitmq/rabbitmq.conf*:
```
default_user = <<rabbitmq.username>>
default_pass = <<rabbitmq.password>>
```

При передаче файла ключи будут заменены известными.