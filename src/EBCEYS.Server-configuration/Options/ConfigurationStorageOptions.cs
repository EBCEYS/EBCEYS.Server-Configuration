using EBCEYS.Server_configuration.ServiceEnvironment;

namespace EBCEYS.Server_configuration.Options
{
    /// <summary>
    /// A <see cref="ConfigurationProcessOptions"/> class.
    /// </summary>
    public class ConfigurationProcessOptions
    {
        /// <summary>
        /// Is enable.
        /// </summary>
        public bool Enable { get; set; }
        /// <summary>
        /// The configuration directory.
        /// </summary>
        public string ConfigDirectory { get; set; }
        /// <summary>
        /// The main process period.
        /// </summary>
        public TimeSpan ProcessPeriod { get; set; }
        /// <summary>
        /// Initiates a new instance of <see cref="ConfigurationProcessOptions"/>.
        /// </summary>
        /// <param name="enable">The enable.</param>
        /// <param name="configDir">The configuration directory.</param>
        /// <param name="processPeriod">The process period. Should be more than <see cref="TimeSpan.Zero"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public ConfigurationProcessOptions(bool enable, string configDir, TimeSpan processPeriod)
        {
            Enable = enable;
            ConfigDirectory = configDir;
            if (!Enable)
            {
                return;
            }
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(processPeriod, TimeSpan.Zero, nameof(processPeriod));
            ProcessPeriod = processPeriod;
        }
        /// <summary>
        /// Initiates a new instance of <see cref="ConfigurationProcessOptions"/> from <see cref="Environment"/>.
        /// </summary>
        /// <returns>A new instance of <see cref="ConfigurationProcessOptions"/>.</returns>
        public static ConfigurationProcessOptions CreateFromEnvironment()
        {
            return new(
                SupportedEnvironmentVariables.ConfigStorageEnable.Value!.Value,
                SupportedEnvironmentVariables.ConfigStorageConfigPath.Value!, 
                SupportedEnvironmentVariables.ConfigStorageProcessPeriod.Value!.Value);
        }
    }
}
