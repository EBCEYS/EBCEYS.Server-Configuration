using System.Formats.Tar;

namespace EBCEYS.Server_configuration.Middle.Archives;

/// <summary>
///     A <see cref="TarArchiveHelper" /> class.
/// </summary>
public class TarArchiveHelper : IArchiveHelper
{
    /// <inheritdoc />
    public async Task<Stream> ArchivateDirectoryAsync(string dirPath, bool includeBaseDir = true)
    {
        MemoryStream ms = new();
        await TarFile.CreateFromDirectoryAsync(dirPath, ms, includeBaseDir);
        ms.Seek(0, SeekOrigin.Begin);
        return ms;
    }

    /// <inheritdoc />
    public async Task ExtractArchiveAsync(Stream tarArchive, string destinationDir, bool overwrite = false)
    {
        DirectoryInfo configDir = new(destinationDir);
        if (!configDir.Exists) configDir.Create();
        await using TarReader reader = new(tarArchive);
        TarEntry? entry;
        while ((entry = await reader.GetNextEntryAsync()) != null)
        {
            var newFile = Path.Combine(configDir.FullName, entry.Name.TrimStart(Path.DirectorySeparatorChar));
            var dirName = Path.GetDirectoryName(newFile)!;
            if (!Directory.Exists(dirName)) Directory.CreateDirectory(dirName);
            if (entry.EntryType == TarEntryType.Directory) continue;
            await entry.ExtractToFileAsync(newFile, overwrite);
        }
    }

    /// <inheritdoc />
    public Task ExtractArchiveAsync(Stream tarArchive, DirectoryInfo destination, bool overwrite = false)
    {
        return ExtractArchiveAsync(tarArchive, destination.FullName, overwrite);
    }
}