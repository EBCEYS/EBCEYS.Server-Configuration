# �������� ��������:

## 1. Configs

�������� � ����������, ������� ����������� � *CONFIG_PROCESSOR_CONFIGS_PATH*.

������ ���� ��� �������� ������������ ����������: */storage/configs/rabbitmq/rabbitmq.conf*, ���:
* */storage/configs/* - ���������� ���� �� ���������� �� *CONFIG_PROCESSOR_CONFIGS_PATH*
* */rabbitmq/* - ��� ���� ����������
* *rabbitmq.conf* - ���� ������������

���� *CONFIG_PROCESSOR_ENABLE=true*, �� ������� ����� �������������� (�������� ��������� ����������) ������������� � ��������� �� ���������� ��������:
* *CONFIG_PROCESSOR_CONTAINER_LABEL_KEY* - �����, ������� ������ ���� ������������ ����� ���� ���������� (*rabbitmq*)
* *CONFIG_PROCESSOR_CONTAINER_CONFIG_PATH_LABEL_KEY* - �����, � ������� ����������� ���� �� ��������� ������������
* *CONFIG_PROCESSOR_CONTAINER_LABEL_RESTART_AFTER* - **������������** �����, ������� ��������� ����� �� ������������� ��������� ����� ���� ��� ��������� ����� ������

���� *CONFIG_PROCESSOR_ENABLE=false*, �� ������� �������������� ������������� �� ����� (������������� ������).

� ���� ������ �� ���������� ����������� �� *WebApi*. ����� *configuration/files/info*, ��� � ���������� �����������:
* containerTypeName - ��� ���� ���������� (*rabbitmq*)
* containerSavePath - ����, ���� ��������� ������� � ����������

**�����!!!** � ������ ������ ������� ������������� �� ����������. �� ���������� �� ����� �������� �� ������� *configuration/files/{filePath}*, ��� *filePath* - ���������� ���� �� ������ ����� �� �������.

## 2. Keys

�������� � ����������, ������� ����������� � *KEYS_STORAGE_PATH*.

������ ���� ��� �������� ������: */storage/keys/rabbitmq.key*.

���� **.key* ������������ �� ���� *json* ��������.

������:
```json
{
  "username": "admin",
  "password": "admin"
}
```

� ����� ������, � ������� ����� ����� ����� ��������� ���:
```
<<rabbitmq.username>> = admin
<<rabbitmq.password>> = admin
```
����� ����� ������������� �������������� � ���������������� ���� ��� ��� �������.

��������:

����� ��������� ���������������� ���� */storage/configs/rabbitmq/rabbitmq.conf*:
```
default_user = <<rabbitmq.username>>
default_pass = <<rabbitmq.password>>
```

��� �������� ����� ����� ����� �������� ����������.