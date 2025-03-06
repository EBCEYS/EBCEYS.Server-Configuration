# WebApi

## *ConfigurationController* */api/configuration/*

### *GET* */files/info?containerTypeName=\{container\}&containerSavePath=\{absSavePath\}*

����� ���������� ���������� � ���������������� ������ ��� ������� ���� ����������.

[����� �� ����������](https://github.com/EBCEYS/EBCEYS.ContainersEnvironment/blob/master/EBCEYS.ContainersEnvironment/Configuration/Models/ConfigurationFileInfo.cs)

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
���:
* *serverFileFullPath* - ���� �� ����� �� �������, �� �������� ����� ���� ���� ���������.
* *lastWriteUTC* - ����� ���������� ��������� �����.
* *containerTypeName* - ��� ���� ���������� (������������ *containerTypeName* �� query).
* *fileSaveFullPath* - ������ ���� ��� ���������� ����� � ����������.

### *GET* */files/\{filePath\}*

����� ������ ���� ������������. *filePath* ������ ���� ���������� � ��������� ���������� �������� ��������.

��������������, ��� ����� ������� �� *serverFileFullPath* �� ����������� �������.

### *GET* */archive/tar?typeName=\{container\}*

������������ tar ����� � ���������.

*typeName* - ������������ ��������. ���� ������, �� ������������ ������� ���������� ����������, ���� ���, �� ��� �������.

### *PATCH* */archive/tar?removeOldFiles=\{true/false\}*

��������� ����� ������� �� ������. ���� *removeOldFiles=true*, �� ������� ��� ������ ������� � ������ �����. ����� �������������� ������������.

## *KeysController* */api/keys*

### *PATCH* */archive/tar?removeOldFiles=\{true/false\}*

��������� ����� ����� �� ������. ���� *removeOldFiles=true*, �� ������� ��� ������ ����� (������ �����, � ������ �������� ���� *KEYS_STORAGE_FORGET_OLD_KEYS=true*). ����� �������������� ������������.

### *GET* */archive/tar*

���������� tar ����� � �������.

## *DockerApiController*

�� ���������� � *release*. ��� ��� ����������� ���.