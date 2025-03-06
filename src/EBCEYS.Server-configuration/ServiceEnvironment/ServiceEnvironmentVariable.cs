using System.Text;
using EBCEYS.ContainersEnvironment.ServiceEnvironment;
using EBCEYS.Server_configuration.Middle;

namespace EBCEYS.Server_configuration.ServiceEnvironment
{
    /// <summary>
    /// The supported environment variables storage.
    /// </summary>
    public static class SupportedEnvironmentVariables
    {
        private const string serviceEnableSwagger = "SERVICE_ENABLE_SWAGGER";
        private const string serviceDatabasePath = "SERVICE_DATABASE_PATH";

        private const string dockerConnectionUseDefaultKey = "DOCKER_CONNECTION_USE_DEFAULT";
        private const string dockerConnectionUrlKey = "DOCKER_CONNECTION_URL";
        private const string dockerConnectionDefaultTimeoutKey = "DOCKER_CONNECTION_DEFAULT_TIMEOUT";

        private const string configStorage_Enable = "CONFIG_PROCESSOR_ENABLE";
        private const string configStorage_ConfigsPath = "CONFIG_PROCESSOR_CONFIGS_PATH";
        private const string configStorage_ProcessPeriod = "CONFIG_PROCESSOR_PROCESS_PERIOD";
        private const string configStorage_ContainerLabelKey = "CONFIG_PROCESSOR_CONTAINER_LABEL_KEY";
        private const string configStorage_ContainerConfigPathLabelKey = "CONFIG_PROCESSOR_CONTAINER_CONFIG_PATH_LABEL_KEY";
        private const string configStorage_ContainerLabelRestartAfter = "CONFIG_PROCESSOR_CONTAINER_LABEL_RESTART_AFTER";

        private const string keysStorage_KeysPath = "KEYS_STORAGE_PATH";
        private const string keysStorage_KeysFileCheckPeriod = "KEYS_STORAGE_FILE_CHECK_PERIOD";
        private const string keysStorage_KeysForgetOldKeys = "KEYS_STORAGE_FORGET_OLD_KEYS";

        private const string dbCleaner_TimeToStore = "DBCLEANER_TIME_TO_STORE";
        /// <summary>
        /// The service database path.
        /// </summary>
        public static ServiceEnvironmentVariable<string> ServiceDatabasePath { get; } = new(serviceDatabasePath, "configuration.db");
        /// <summary>
        /// The service enable swagger.
        /// </summary>
        public static ServiceEnvironmentVariable<bool?> ServiceEnableSwagger { get; } = new(serviceEnableSwagger, true);
        /// <summary>
        /// The docker connection use default.<br/>
        /// <c>true</c> if <see cref="DockerController"/> should use default connection;<br/>
        /// otherwise <see cref="DockerConnectionUrl"/> will be used.
        /// </summary>
        public static ServiceEnvironmentVariable<bool?> DockerConnectionUseDefault { get; } = new
            (
            dockerConnectionUseDefaultKey,
            true,
            $"If set true: docker client will use localhost connection; otherwise connection from {dockerConnectionUrlKey}"
            );
        /// <summary>
        /// The docker connection url.
        /// </summary>
        public static ServiceEnvironmentVariable<string> DockerConnectionUrl { get; } = new
            (
            dockerConnectionUrlKey,
            "unix:///var/run/docker.sock",
            $"Param will be ignored if {dockerConnectionUseDefaultKey} set true"
            );
        /// <summary>
        /// The docker connection timeout.
        /// </summary>
        public static ServiceEnvironmentVariable<TimeSpan?> DockerConnectionTimeout { get; } = new
            (
            dockerConnectionDefaultTimeoutKey, 
            TimeSpan.FromSeconds(10.0)
            );
        /// <summary>
        /// The config storage enable.
        /// </summary>
        public static ServiceEnvironmentVariable<bool?> ConfigStorageEnable { get; } = new(configStorage_Enable, true);
        /// <summary>
        /// The config storage config path.
        /// </summary>
        public static ServiceEnvironmentVariable<string> ConfigStorageConfigPath { get; } = new
            (
            configStorage_ConfigsPath,
            "/storage/configs"
            );
        /// <summary>
        /// The config storage container label key.
        /// </summary>
        public static ServiceEnvironmentVariable<string> ConfigStorageContainerLabelKey { get; } = new
            (
            configStorage_ContainerLabelKey,
            "configuration.service.type.name"
            );
        /// <summary>
        /// The config storage container config path label key.
        /// </summary>
        public static ServiceEnvironmentVariable<string> ConfigStorageContainerConfigPathLabelKey { get; } = new
            (
            configStorage_ContainerConfigPathLabelKey,
            "configuration.file.path"
            );
        /// <summary>
        /// The config storage container label restart after.
        /// </summary>
        public static ServiceEnvironmentVariable<string> ConfigStorageContainerLabelRestartAfter { get; } = new
            (
            configStorage_ContainerLabelRestartAfter,
            "configuration.restartafter"
            );
        /// <summary>
        /// The config storage process period.
        /// </summary>
        public static ServiceEnvironmentVariable<TimeSpan?> ConfigStorageProcessPeriod { get; } = new
            (
            configStorage_ProcessPeriod,
            TimeSpan.FromSeconds(5.0)
            );
        /// <summary>
        /// The keys storage keys path.
        /// </summary>
        public static ServiceEnvironmentVariable<string> KeysStorageKeysPath { get; } = new
            (
            keysStorage_KeysPath,
            "/storage/keys"
            );
        /// <summary>
        /// The keys storage key files check period.
        /// </summary>
        public static ServiceEnvironmentVariable<TimeSpan?> KeysStorageKeysFilesCheckPeriod { get; } = new
            (
            keysStorage_KeysFileCheckPeriod,
            TimeSpan.FromSeconds(5.0)
            );
        /// <summary>
        /// The keys storage key forget old keys.
        /// </summary>
        public static ServiceEnvironmentVariable<bool?> KeysStorageForgetOldKeys { get; } = new
            (
            keysStorage_KeysForgetOldKeys,
            false
            );
        /// <summary>
        /// The dbcleaner time to store unexisted containers.
        /// </summary>
        public static ServiceEnvironmentVariable<TimeSpan?> DBCleanerTimeToStore { get; } = new
            (
            dbCleaner_TimeToStore,
            TimeSpan.Zero,
            "Time to store information about unexisted containers in database."
            );
        /// <summary>
        /// Gets a full info of all supported <see cref="ServiceEnvironmentVariable{T}"/>.
        /// </summary>
        /// <returns>Collection with supported <see cref="ServiceEnvironmentInfo"/>.</returns>
        public static IEnumerable<ServiceEnvironmentInfo> Info
        {
            get
            {
                List<ServiceEnvironmentInfo> infos = DefaultEnvironmentVariables.GetVariablesInfo().ToList();
                infos.AddRange(
                    [
                    ServiceDatabasePath.GetInfo(),
                    ServiceEnableSwagger.GetInfo(),
                    DockerConnectionUseDefault.GetInfo(),
                    DockerConnectionUrl.GetInfo(),
                    DockerConnectionTimeout.GetInfo(),

                    ConfigStorageEnable.GetInfo(),
                    ConfigStorageConfigPath.GetInfo(),
                    ConfigStorageProcessPeriod.GetInfo(),
                    ConfigStorageContainerLabelKey.GetInfo(),
                    ConfigStorageContainerConfigPathLabelKey.GetInfo(),
                    ConfigStorageContainerLabelRestartAfter.GetInfo(),

                    KeysStorageKeysPath.GetInfo(),
                    KeysStorageKeysFilesCheckPeriod.GetInfo(),
                    KeysStorageForgetOldKeys.GetInfo(),

                    DBCleanerTimeToStore.GetInfo(),
                    ]
                );
                return infos;
            }
        }

        /// <summary>
        /// Gets the string representation of supported <see cref="ServiceEnvironmentVariable{T}"/>.
        /// </summary>
        /// <returns></returns>
        public static string GetHelp()
        {
            StringBuilder sb = new();
            sb.AppendLine("Supported environment variables:");
            foreach (ServiceEnvironmentInfo info in Info)
            {
                sb.AppendLine(info.ToString());
            }
            return sb.ToString();
        }

    }
}
