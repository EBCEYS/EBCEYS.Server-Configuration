namespace EBCEYS.Server_configuration.Middle.Archives
{
    /// <summary>
    /// A <see cref="IArchiveHelper"/> interface.
    /// </summary>
    public interface IArchiveHelper
    {
        /// <summary>
        /// Archivates a directory.
        /// </summary>
        /// <param name="dirPath">The directory to archivate path.</param>
        /// <param name="includeBaseDir">Include base directory?</param>
        /// <returns>A <see cref="Stream"/> of archive.</returns>
        Task<Stream> ArchivateDirectoryAsync(string dirPath, bool includeBaseDir = true);
        /// <summary>
        /// Extracts the archive to directory with full direcotry creations.
        /// </summary>
        /// <param name="archive">The archive stream.</param>
        /// <param name="destination">The destination directory.</param>
        /// <param name="overwrite">Do overwrite existing files?</param>
        /// <returns></returns>
        Task ExtractArchiveAsync(Stream archive, DirectoryInfo destination, bool overwrite = false);
        /// <summary>
        /// Extracts the archive to directory with full directory creations.
        /// </summary>
        /// <param name="archive">The archive stream.</param>
        /// <param name="destinationDir">The destination directory.</param>
        /// <param name="overwrite">Do overwrite existing files?</param>
        /// <returns></returns>
        Task ExtractArchiveAsync(Stream archive, string destinationDir, bool overwrite = false);
    }
}