using System.Text;
using Docker.DotNet.Models;
using EBCEYS.ContainersEnvironment.Configuration.Models;
using EBCEYS.ContainersEnvironment.Extensions;
using EBCEYS.Server_configuration.ConfigDatabase;
using EBCEYS.Server_configuration.Middle.Archives;
using EBCEYS.Server_configuration.Middle.Models;
using EBCEYS.Server_configuration.Options;
using EBCEYS.Server_configuration.ServiceEnvironment;
using Microsoft.EntityFrameworkCore;

namespace EBCEYS.Server_configuration.Middle;

/// <summary>
///     A <see cref="ConfigurationProcessingService" /> class.
/// </summary>
/// <remarks>
///     Initiates a new instance of <see cref="ConfigurationProcessingService" />.
/// </remarks>
/// <param name="logger">The logger.</param>
/// <param name="db">The database context.</param>
/// <param name="docker">The docker controller.</param>
/// <param name="keys">The keys storage.</param>
/// <param name="archiveHelper"></param>
/// <param name="opts">The options.</param>
/// <param name="dbCleanerOpts">The db cleaner options.</param>
public class ConfigurationProcessingService(
    ILogger<ConfigurationProcessingService> logger,
    ConfigurationDatabaseContext db,
    DockerController docker,
    KeysStorageService keys,
    IArchiveHelper archiveHelper,
    ConfigurationProcessOptions? opts = null,
    DBCleanerOptions? dbCleanerOpts = null) : BackgroundService
{
    private readonly DBCleanerOptions _cleanerOpts = dbCleanerOpts ?? DBCleanerOptions.CreateFromEnvironment();
    private readonly ConfigurationProcessOptions _opts = opts ?? ConfigurationProcessOptions.CreateFromEnvironment();

    /// <summary>
    ///     Indicates that service is updating configs now.
    /// </summary>
    public bool IsNowUpdatingConfig { get; private set; }

    //will refactor this later
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (true)
        {
            logger.LogInformation("Run service without {this}", GetType().Name);
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            if (IsNowUpdatingConfig)
            {
                await Task.Delay(_opts.ProcessPeriod, stoppingToken);
                continue;
            }

            var configsDir = Directory.CreateDirectory(_opts.ConfigDirectory);

            try
            {
                var containers = await docker.GetAllContainersAsync(true, stoppingToken);
                foreach (var container in containers)
                {
                    if (!container.Labels.TryGetValue(
                            SupportedEnvironmentVariables.ConfigStorageContainerConfigPathLabelKey.Value,
                            out var containerConfigLabel) || string.IsNullOrWhiteSpace(containerConfigLabel)) continue;
                    if (!container.Labels.TryGetValue(
                            SupportedEnvironmentVariables.ConfigStorageContainerLabelKey.Value,
                            out var containerName) || string.IsNullOrWhiteSpace(containerName)) continue;

                    await using var transaction = await db.Database.BeginTransactionAsync(stoppingToken);
                    try
                    {
                        var hasChanges = await ProcessContainer(configsDir, container, containerConfigLabel,
                            containerName, stoppingToken);
                        await db.SaveChangesAsync(stoppingToken);
                        await transaction.CommitAsync(stoppingToken);
                        var doRestart =
                            container.Labels.GetLabel<bool?>(SupportedEnvironmentVariables
                                .ConfigStorageContainerLabelRestartAfter.Value!)?.Value ?? true;
                        if (hasChanges && doRestart)
                            await docker.RestartContainerAsync(new ContainerInfo(container.ID), stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync(stoppingToken);
                        logger.LogError(ex, "Error on processing container {name}", container.Names.First());
                    }
                }

                try
                {
                    await RemoveOldAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error on dbcleaner processing!");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on config processing!");
            }

            await Task.Delay(_opts.ProcessPeriod, stoppingToken);
        }
    }

    [Obsolete]
    private async Task<bool> ProcessContainer(DirectoryInfo configsDir, ContainerListResponse container,
        string containerConfigLabel, string containerName, CancellationToken stoppingToken)
    {
        var dbContainer =
            await db.Containers.FirstOrDefaultAsync(c => c.Id == container.ID && c.Name == containerName,
                stoppingToken);
        var containerConfigs = await db.Configs.AsNoTracking().Where(cc => cc.ContainerId == container.ID)
            .ToListAsync(stoppingToken);

        dbContainer = await CreateOrUpdateContainerAsync(container, containerConfigLabel, containerName, dbContainer,
            stoppingToken);

        DirectoryInfo containerConfigDir = new(Path.Combine(configsDir.FullName, containerName));
        if (!containerConfigDir.Exists)
        {
            logger.LogDebug("No config files for container {name}", containerName);
            return false;
        }

        var thisContainerServiceConfigFiles = containerConfigDir.EnumerateFiles("*", SearchOption.AllDirectories);
        if (!thisContainerServiceConfigFiles.Any())
        {
            logger.LogDebug("No config files for container {name}", containerName);
            return false;
        }

        var filesToSendToContainer = await GetConfigFilesToSend(containerConfigDir, container, containerConfigLabel,
            dbContainer, containerConfigs, thisContainerServiceConfigFiles, stoppingToken);
        if (filesToSendToContainer.Count == 0) return false;
        var tempConfigDirectory =
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "tmp", containerName));
        await SendFilesToContainer(container, containerConfigLabel, containerConfigDir, filesToSendToContainer,
            tempConfigDirectory, stoppingToken);
        logger.LogDebug("Remove temp directory for container {id}", container.ID);
        tempConfigDirectory.Delete(true);
        await CreateOrUpdateContainerConfigs(filesToSendToContainer, stoppingToken);
        return true;
    }

    [Obsolete]
    private async Task CreateOrUpdateContainerConfigs(List<ContainerConfigFileInfo> filesToSendToContainer,
        CancellationToken token = default)
    {
        foreach (var f in filesToSendToContainer.Where(f => f.DbEntity != null && f.DbEntity.FilePath != default))
        {
            if (f.DbEntity.FileMTime == DateTime.MinValue)
            {
                var stats = await docker.GetFileStatFromContainerAsync(new ContainerInfo(f.ContainerId), f.Destination,
                    false, token);
                if (stats == null)
                {
                    logger.LogWarning("File {file} not found in {dest}! Error on copy!", f.ConfigFile.FullName,
                        f.Destination);
                    continue;
                }

                f.DbEntity.FileMTime = stats.Mtime;
            }

            var existedConfig = await db.Configs.FirstOrDefaultAsync(c => c.FilePath == f.DbEntity.FilePath, token);
            if (existedConfig == null)
            {
                await db.Configs.AddAsync(f.DbEntity, token);
                continue;
            }

            existedConfig.ContainerFilePath = f.DbEntity.ContainerFilePath;
            existedConfig.FileLastUpdate = f.DbEntity.FileLastUpdate;
            existedConfig.FileMTime = f.DbEntity.FileMTime;
            existedConfig.IsExists = f.DbEntity.IsExists;
        }
    }

    /// <summary>
    ///     Gets the config info for container if exists.
    /// </summary>
    /// <param name="containerTypeName">The container type name.</param>
    /// <param name="containerSaveConfDirectory">The container save configuration directory.</param>
    /// <returns>
    ///     Collection of the <see cref="ConfigurationFileInfo" /> if exists; otherwise
    ///     <see cref="Enumerable.Empty{TResult}" />
    /// </returns>
    public IEnumerable<ConfigurationFileInfo> GetConfigInfoForContainer(string containerTypeName,
        string containerSaveConfDirectory)
    {
        DirectoryInfo configs = new(_opts.ConfigDirectory);
        if (!configs.Exists)
            //No configs
            return [];
        var containerConfigDirs = configs.EnumerateDirectories().Where(d => d.Name == containerTypeName);
        if (!containerConfigDirs.Any()) return [];
        List<ConfigurationFileInfo> result = [];
        foreach (var dir in containerConfigDirs)
        {
            var files = dir.EnumerateFiles("*", SearchOption.AllDirectories)
                .Select(f => GetConfigInfoForFile(f, containerTypeName,
                    FormatFilePathForContainer(f, containerSaveConfDirectory, dir.FullName)));
            result.AddRange(files);
        }

        return result;
    }

    /// <summary>
    ///     Gets the config file with replaced keys.
    /// </summary>
    /// <param name="filePath">The config file path.</param>
    /// <returns>The config file with replaced keys if exists; otherwise <c>null</c>.</returns>
    public async Task<Stream?> GetConfigurationFile(string filePath)
    {
        if (IsNowUpdatingConfig) return null;
        DirectoryInfo configs = new(_opts.ConfigDirectory);
        if (!configs.Exists) return null;
        if (!filePath.StartsWith(configs.FullName)) return null;

        if (!File.Exists(filePath)) return null;
        using var sr = File.OpenText(filePath);
        MemoryStream result = new();
        await using StreamWriter sw = new(result, Encoding.UTF8, 1024, true);
        var cachedKeys = keys.GetKeys();
        while (await sr.ReadLineAsync() is { } line)
        {
            foreach (var key in cachedKeys) line = line.Replace(key.Key, key.Value);
            await sw.WriteLineAsync(line);
        }

        result.Seek(0, SeekOrigin.Begin);
        return result;
    }

    /// <summary>
    ///     Gets the configuration tar archive stream of <paramref name="containerType" />.<br />
    ///     Returns tar archive stream with all configs if <paramref name="containerType" /> is <c>null</c>.
    /// </summary>
    /// <param name="containerType">The container type.[optional]</param>
    /// <returns>
    ///     <see cref="Stream" /> of tar archive with <paramref name="containerType" /> configuration if exists; otherwise
    ///     <c>null</c>.
    /// </returns>
    public async Task<Stream?> GetContainerTypeConfigArchive(string? containerType = null)
    {
        if (IsNowUpdatingConfig) return null;
        DirectoryInfo configDir = new(_opts.ConfigDirectory);
        if (!configDir.Exists) return null;
        var containerConfDir = containerType != null
            ? configDir.EnumerateDirectories(containerType, SearchOption.TopDirectoryOnly).FirstOrDefault()
            : configDir;
        if (containerConfDir == null || !containerConfDir.Exists) return null;
        return await archiveHelper.ArchivateDirectoryAsync(containerConfDir.FullName, false);
    }

    /// <summary>
    ///     Patchs the configs.
    /// </summary>
    /// <param name="tarArchive"></param>
    /// <param name="removeOldFiles"></param>
    /// <returns></returns>
    public async Task<bool> PatchConfigs(Stream tarArchive, bool removeOldFiles)
    {
        IsNowUpdatingConfig = true;
        try
        {
            DirectoryInfo configDir = new(_opts.ConfigDirectory);
            if (!configDir.Exists) configDir.Create();
            var tmpConfDir = configDir.Parent?.CreateSubdirectory("tmpconfig") ??
                             Directory.CreateDirectory(Path.Combine("/", "tmpConfigs"));
            try
            {
                await archiveHelper.ExtractArchiveAsync(tarArchive, tmpConfDir, true);
                if (removeOldFiles)
                {
                    configDir.Delete(true);
                    tmpConfDir.MoveTo(configDir.FullName);
                }
                else
                {
                    CopyFilesToNewDir(tmpConfDir, configDir, true);
                    tmpConfDir.Delete(true);
                }
            }
            catch (Exception)
            {
                tmpConfDir.Delete(true);
                throw;
            }

            IsNowUpdatingConfig = false;
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error on extracting archive!");
            IsNowUpdatingConfig = false;
            return false;
        }
    }

    private static void CopyFilesToNewDir(DirectoryInfo orig, DirectoryInfo dest, bool overwrite)
    {
        if (!orig.Exists) return;
        dest.Create();
        foreach (var file in orig.EnumerateFiles("*", SearchOption.TopDirectoryOnly))
        {
            var newPath = Path.Combine(dest.FullName, file.Name);
            file.CopyTo(newPath, overwrite);
        }

        foreach (var dir in orig.EnumerateDirectories("*", SearchOption.TopDirectoryOnly))
        {
            DirectoryInfo newDestDir = new(Path.Combine(dest.FullName, dir.Name));
            CopyFilesToNewDir(dir, newDestDir, overwrite);
        }
    }

    private static ConfigurationFileInfo GetConfigInfoForFile(FileInfo file, string containerTypeName,
        string containerSavePath)
    {
        return new ConfigurationFileInfo(file.FullName, file.LastWriteTimeUtc, containerTypeName, containerSavePath);
    }

    private static string FormatFilePathForContainer(FileInfo confFile, string containerSaveDir, string configDirectory)
    {
        return Path.Combine(containerSaveDir,
            confFile.FullName.Replace(configDirectory, "").TrimStart(Path.DirectorySeparatorChar));
    }

    private async Task SendFilesToContainer(ContainerListResponse container, string containerConfigLabel,
        DirectoryInfo containerConfigDir, List<ContainerConfigFileInfo> filesToSendToContainer,
        DirectoryInfo tempConfigDirectory, CancellationToken stoppingToken)
    {
        await CopyFilesToNewDirectoryWithReplacingKeys(containerConfigDir,
            filesToSendToContainer.Select(f => f.ConfigFile.FullName), tempConfigDirectory, keys.GetKeys(),
            stoppingToken);
        try
        {
            await docker.CopyFilesToContainerAsync(new ContainerInfo(container.ID), containerConfigLabel,
                tempConfigDirectory, stoppingToken);
            logger.LogDebug("Sending files from {dir} to container {id} completed!", tempConfigDirectory.FullName,
                container.ID);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error on copiing files to container!");
        }
    }

    private async Task<List<ContainerConfigFileInfo>> GetConfigFilesToSend(DirectoryInfo configsDir,
        ContainerListResponse container, string containerConfigLabel, ContainerDBEntity dbContainer,
        List<ConfigurationDBEntity> configs, IEnumerable<FileInfo> thisContainerServiceConfigFiles,
        CancellationToken stoppingToken)
    {
        List<ContainerConfigFileInfo> filesToSendToContainer = [];
        foreach (var file in thisContainerServiceConfigFiles)
        {
            var destPath = GetContainerConfigPath(configsDir, file, containerConfigLabel);
            ContainerConfigFileInfo ccfi = new(container.ID, file, destPath);
            var stats = await docker.GetFileStatFromContainerAsync(new ContainerInfo(container.ID), destPath, false,
                stoppingToken);
            if (stats == null)
            {
                ccfi.DbEntity = new ConfigurationDBEntity
                {
                    ContainerFilePath = destPath,
                    FilePath = file.FullName,
                    IsExists = false,
                    ContainerId = dbContainer.Id,
                    FileLastUpdate = file.LastWriteTimeUtc,
                    FileMTime = DateTime.MinValue
                };
                filesToSendToContainer.Add(ccfi);
                continue;
            }

            var config = configs.FirstOrDefault(c => c.ContainerId == container.ID && c.ContainerFilePath == destPath);
            if (config == null || config.FileLastUpdate < file.LastWriteTimeUtc)
            {
                config ??= new ConfigurationDBEntity
                {
                    ContainerFilePath = destPath,
                    ContainerId = container.ID,
                    FileLastUpdate = file.LastWriteTimeUtc,
                    FileMTime = stats.Mtime,
                    FilePath = file.FullName,
                    IsExists = true
                };
                config.IsExists = true;
                config.FileLastUpdate = file.LastWriteTimeUtc;
                config.FileMTime = stats.Mtime;
                ccfi.MTime = stats.Mtime;
                ccfi.DbEntity = config;
                filesToSendToContainer.Add(ccfi);
            }
        }

        return filesToSendToContainer;
    }

    private async Task<ContainerDBEntity> CreateOrUpdateContainerAsync(ContainerListResponse container,
        string containerConfigLabel, string containerName, ContainerDBEntity? dbContainer,
        CancellationToken stoppingToken)
    {
        if (dbContainer == null)
        {
            dbContainer = new ContainerDBEntity
            {
                Id = container.ID,
                Name = containerName,
                ConfigurationPath = containerConfigLabel,
                IsExists = true
            };
            logger.LogDebug("Add container {id} to db", container.ID);
            await db.Containers.AddAsync(dbContainer, stoppingToken);
            return dbContainer;
        }

        if (containerConfigLabel != dbContainer.ConfigurationPath)
        {
            dbContainer.ConfigurationPath = containerConfigLabel;
            logger.LogDebug("Change container {name} configuration path to {newPath}", dbContainer.Id,
                containerConfigLabel);
        }

        return dbContainer;
    }

    private static string GetContainerConfigPath(DirectoryInfo configDir, FileInfo configFile,
        string containerConfigPath)
    {
        return Path.Combine(containerConfigPath,
            configFile.FullName.Replace($"{configDir.FullName}{Path.DirectorySeparatorChar}", ""));
    }

    private async Task CopyFilesToNewDirectoryWithReplacingKeys(DirectoryInfo filesDir, IEnumerable<string> fileNames,
        DirectoryInfo newDir, IReadOnlyDictionary<string, string> containerKeys, CancellationToken token = default)
    {
        var files = filesDir.EnumerateFiles("*", SearchOption.TopDirectoryOnly)
            .Where(f => fileNames.Contains(f.FullName));
        newDir.Create();
        foreach (var file in files) await CopyFileToNewDirWithReplacingKeys(newDir, containerKeys, file, token);
        foreach (var dir in filesDir.EnumerateDirectories("*", SearchOption.TopDirectoryOnly))
        {
            var nextNewDir = Directory.CreateDirectory(Path.Combine(newDir.FullName, dir.Name));
            await CopyFilesToNewDirectoryWithReplacingKeys(dir, fileNames, nextNewDir, containerKeys, token);
        }
    }

    private async Task CopyFileToNewDirWithReplacingKeys(DirectoryInfo newDir, IReadOnlyDictionary<string, string> keysToReplace,
        FileInfo file, CancellationToken token)
    {
        using var sr = File.OpenText(file.FullName);
        FileInfo newFile = new(Path.Combine(newDir.FullName, file.Name));

        await using var sw = File.CreateText(newFile.FullName);
        logger.LogDebug("Copy file {file} to {newFile} with replacing keys", file.FullName, newFile.FullName);
        while (await sr.ReadLineAsync(token) is { } line)
        {
            foreach (var key in keysToReplace) line = line.Replace(key.Key, key.Value);
            await sw.WriteLineAsync(line);
        }
    }

    [Obsolete]
    private async Task RemoveOldAsync(CancellationToken stoppingToken = default)
    {
        var dockerContainersTask = docker.GetAllContainersAsync(true, stoppingToken);
        var containersInDb = db.Containers.Include(c => c.Configs).ToListAsync(stoppingToken);

        Task.WaitAll([dockerContainersTask, containersInDb], stoppingToken);

        var dockerContainers = dockerContainersTask.Result.ToList();

        var now = DateTimeOffset.UtcNow;

        containersInDb.Result.ForEach(async c =>
        {
            var container = dockerContainers.FirstOrDefault(dc =>
                string.Compare(dc.ID, c.Id, StringComparison.CurrentCultureIgnoreCase) == 0);
            if (container == null)
            {
                c.IsExists = false;
                c.DeletionUTC ??= now;
                logger.LogInformation("Container {name} marked for deletion from db", c.Id);
                if (_cleanerOpts.TimeToStoreUnexistedContainers == TimeSpan.Zero ||
                    c.DeletionUTC.Value.Add(_cleanerOpts.TimeToStoreUnexistedContainers) < now)
                {
                    logger.LogInformation("Remove container {id} from db and his configs...", c.Id);
                    db.Configs.RemoveRange(c.Configs);
                    db.Containers.Remove(c);
                }
            }
            else
            {
                if (!c.IsExists)
                {
                    logger.LogInformation("Container {id} is unmarked for deletion", c.Id);
                    c.IsExists = true;
                    c.DeletionUTC = null;
                }

                foreach (var configFile in c.Configs)
                {
                    var stats = await docker.GetFileStatFromContainerAsync(new ContainerInfo(c.Id),
                        configFile.ContainerFilePath, false, stoppingToken);
                    if (stats == null)
                    {
                        configFile.IsExists = false;
                        continue;
                    }

                    configFile.FileMTime = stats.Mtime;
                }
            }
        });
        await db.SaveChangesAsync(stoppingToken);
    }

    private class ContainerConfigFileInfo(string containerId, FileInfo configFile, string dest)
    {
        public FileInfo ConfigFile { get; } = configFile;
        public string Destination { get; } = dest;
        public DateTime MTime { get; set; }
        public string ContainerId { get; } = containerId;
        public ConfigurationDBEntity DbEntity { get; set; } = new();
    }
}