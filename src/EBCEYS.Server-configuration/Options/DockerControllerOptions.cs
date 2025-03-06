using EBCEYS.Server_configuration.ServiceEnvironment;

namespace EBCEYS.Server_configuration.Options
{
    /// <summary>
    /// A <see cref="DockerControllerOptions"/> class.
    /// </summary>
    /// <remarks>
    /// Initiates a new instance of <see cref="DockerControllerOptions"/>.
    /// </remarks>
    /// <param name="useDefaultConnection">The use default connection.</param>
    /// <param name="connectionUrl">The connection url.</param>
    /// <param name="timeout">The timeout.</param>
    public class DockerControllerOptions(bool useDefaultConnection = true, string? connectionUrl = "", TimeSpan timeout = default)
    {
        /// <summary>
        /// Indicates a ussage of default docker connection.
        /// </summary>
        public bool UseDefaultConnection { get; } = useDefaultConnection;
        /// <summary>
        /// The connection url. [optional if <see cref="UseDefaultConnection"/> is <c>true</c>].
        /// </summary>
        public string? ConnectionUrl { get; } = connectionUrl;
        /// <summary>
        /// The timeout. Should be more than <see cref="TimeSpan.Zero"/>.
        /// </summary>
        public TimeSpan Timeout { get; } = timeout <= TimeSpan.Zero ? TimeSpan.FromSeconds(10.0) : timeout;
        /// <summary>
        /// Initates a new instance of <see cref="DockerControllerOptions"/> with <see cref="SupportedEnvironmentVariables"/>.
        /// </summary>
        /// <returns>A new instance of <see cref="DockerControllerOptions"/>.</returns>
        public static DockerControllerOptions CreateFromEnvironment()
        {
            return new
                (
                SupportedEnvironmentVariables.DockerConnectionUseDefault.Value!.Value,
                SupportedEnvironmentVariables.DockerConnectionUrl.Value,
                SupportedEnvironmentVariables.DockerConnectionTimeout.Value!.Value
                );
        }
    }
}
