using EBCEYS.Server_configuration.ServiceEnvironment;

namespace EBCEYS.Server_configuration.Options
{
    /// <summary>
    /// A <see cref="KeysStorageOptions"/> class.
    /// </summary>
    public class KeysStorageOptions
    {
        /// <summary>
        /// The keys directory path.
        /// </summary>
        public string KeysDirPath { get; }
        /// <summary>
        /// The check key files period.
        /// </summary>
        public TimeSpan CheckKeyFilesPeriod { get; }
        /// <summary>
        /// Do forget old keys.
        /// </summary>
        public bool ForgetOldKeys { get; }
        /// <summary>
        /// Initiates a new instance of <see cref="KeysStorageOptions"/>.
        /// </summary>
        /// <param name="keysDirPath"></param>
        /// <param name="checkKeyFilesPeriod"></param>
        /// <param name="forgetOldKeys"></param>
        /// <exception cref="ArgumentException"></exception>
        public KeysStorageOptions(string keysDirPath, TimeSpan checkKeyFilesPeriod, bool forgetOldKeys)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(keysDirPath);
            KeysDirPath = keysDirPath;
            CheckKeyFilesPeriod = checkKeyFilesPeriod > TimeSpan.Zero ? checkKeyFilesPeriod : SupportedEnvironmentVariables.KeysStorageKeysFilesCheckPeriod.DefaultValue!.Value;
            ForgetOldKeys = forgetOldKeys;
        }
        /// <summary>
        /// Creates an instance of <see cref="KeysStorageOptions"/> from <see cref="SupportedEnvironmentVariables"/>.
        /// </summary>
        /// <returns>A new instance of <see cref="KeysStorageOptions"/>.</returns>
        public static KeysStorageOptions CreateFromEnvironment()
        {
            return new
                (
                SupportedEnvironmentVariables.KeysStorageKeysPath.Value!, 
                SupportedEnvironmentVariables.KeysStorageKeysFilesCheckPeriod.Value!.Value,
                SupportedEnvironmentVariables.KeysStorageForgetOldKeys.Value!.Value
                );
        }
    }
}
