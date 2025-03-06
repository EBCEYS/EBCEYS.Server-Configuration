using EBCEYS.Server_configuration.ServiceEnvironment;

namespace EBCEYS.Server_configuration.Options
{
    /// <summary>
    /// A <see cref="DBCleanerOptions"/> class.
    /// </summary>
    public class DBCleanerOptions
    {
        /// <summary>
        /// The time to store unexisted containers.
        /// </summary>
        public TimeSpan TimeToStoreUnexistedContainers { get; set; }
        /// <summary>
        /// Initiates a new instance of <see cref="DBCleanerOptions"/>;
        /// </summary>
        /// <param name="timeToStoreUnexistedContainers">The time to store unexisted containers.</param>
        public DBCleanerOptions(TimeSpan timeToStoreUnexistedContainers)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(timeToStoreUnexistedContainers, TimeSpan.Zero, nameof(timeToStoreUnexistedContainers));
            TimeToStoreUnexistedContainers = timeToStoreUnexistedContainers;
        }
        /// <summary>
        /// Initiates a new instance of <see cref="DBCleanerOptions"/> with params from <see cref="SupportedEnvironmentVariables"/>.
        /// </summary>
        /// <returns>A new instance of <see cref="DBCleanerOptions"/>.</returns>
        public static DBCleanerOptions CreateFromEnvironment()
        {
            return new(SupportedEnvironmentVariables.DBCleanerTimeToStore.Value!.Value);
        }
    }
}
