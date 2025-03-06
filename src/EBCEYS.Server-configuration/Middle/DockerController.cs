using System.Collections.Concurrent;
using System.Formats.Tar;
using Docker.DotNet;
using Docker.DotNet.Models;
using EBCEYS.Server_configuration.Middle.Models;
using EBCEYS.Server_configuration.Options;

namespace EBCEYS.Server_configuration.Middle
{
    /// <summary>
    /// A <see cref="DockerController"/> class.
    /// </summary>
    public class DockerController : IDisposable
    {
        private readonly DockerClient client;
        private readonly ConcurrentDictionary<string, ContainerListResponse> cachedContainers = [];
        /// <summary>
        /// Initiates a new instance of <see cref="DockerController"/>.
        /// </summary>
        /// <param name="opts">The docker controller options. By default creates from environment.</param>
        public DockerController(DockerControllerOptions? opts = null)
        {
            opts ??= DockerControllerOptions.CreateFromEnvironment();
            if (opts.UseDefaultConnection)
            {
                client = new DockerClientConfiguration(defaultTimeout: opts.Timeout).CreateClient();
            }
            else
            {
                client = new DockerClientConfiguration(new Uri(opts.ConnectionUrl!), defaultTimeout: opts.Timeout).CreateClient();
            }
        }
        /// <summary>
        /// Gets all containers list.
        /// </summary>
        /// <param name="refreshCache"><c>true</c> if you want to refresh cached containers info; otherwise<br/>
        /// Will return cached values.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>Collection of <see cref="ContainerListResponse"/> from docker if <paramref name="refreshCache"/> set <c>true</c>; otherwise <br/>
        /// Collection of <see cref="ContainerListResponse"/> from cache.</returns>
        public async Task<IEnumerable<ContainerListResponse>> GetAllContainersAsync(bool refreshCache, CancellationToken token = default)
        {
            if (refreshCache)
            {
                IList<ContainerListResponse> containers = await client.Containers.ListContainersAsync(new()
                {
                    All = true
                }, token);
                cachedContainers.Clear();
                foreach (ContainerListResponse container in containers)
                {
                    cachedContainers.TryAdd(container.ID, container);
                }
                return containers;
            }
            return cachedContainers.Select(c => c.Value);
        }
        /// <summary>
        /// Copies files to docker container.
        /// </summary>
        /// <param name="container">The container id.</param>
        /// <param name="containerDirectoryPath">The container destination directory path.</param>
        /// <param name="token">The cancellation token.</param>
        /// <param name="fileToCopyPath">The files to copy.</param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public async Task CopyFilesToContainerAsync(ContainerInfo container, string containerDirectoryPath, CancellationToken token = default, params FileInfo[] fileToCopyPath)
        {
            await using MemoryStream tarStream = new();
            await using TarWriter tarArchive = new(tarStream);
            foreach (FileInfo file in fileToCopyPath)
            {
                if (!file.Exists)
                {
                    throw new FileNotFoundException("File to copy not found!", file.FullName);
                }
                await tarArchive.WriteEntryAsync(file.FullName, Path.Combine(containerDirectoryPath, file.Name), token);
            }
            tarStream.Position = 0;
            await client.Containers.ExtractArchiveToContainerAsync(container.Id, new()
            {
                //Path = containerDirectoryPath,
                Path = "/",
                AllowOverwriteDirWithFile = true,
            }, tarStream, token);

            
        }
        /// <summary>
        /// Recursivly copies files from <paramref name="dir"/> to <paramref name="container"/>.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="containerDestDir">The container destination directory.</param>
        /// <param name="dir">The directory with files.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns></returns>
        public async Task CopyFilesToContainerAsync(ContainerInfo container, string containerDestDir, DirectoryInfo dir, CancellationToken token = default)
        {
            FileInfo[] files = dir.GetFiles();
            await CopyFilesToContainerAsync(container, containerDestDir, token, files);
            foreach (DirectoryInfo nextDir in dir.EnumerateDirectories())
            {
                await CopyFilesToContainerAsync(container, Path.Combine(containerDestDir, dir.Name), nextDir, token);
            }
        }
        /// <summary>
        /// Copies files to docker container.
        /// </summary>
        /// <param name="container">The container id.</param>
        /// <param name="containerDirectoryPath">The container destination directory path.</param>
        /// <param name="token">The cancellation token.</param>
        /// <param name="fileToCopyPath">The files to copy.</param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public async Task CopyFilesToContainerAsync(ContainerInfo container, string containerDirectoryPath, CancellationToken token = default, params string[] fileToCopyPath)
        {
            FileInfo[] files = fileToCopyPath.Select(f => new FileInfo(f)).ToArray();
            await CopyFilesToContainerAsync(container, containerDirectoryPath, token, files);
        }
        /// <summary>
        /// Gets the file from container. If <paramref name="filePath"/> is directory, will return a first entry.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="token">The cancellation token;</param>
        /// <returns>File <see cref="Stream"/> if file exists; otherwise <c>null</c>.</returns>
        public async Task<Stream?> GetFileFromContainerAsync(ContainerInfo container, string filePath, CancellationToken token = default)
        {
            GetArchiveFromContainerResponse fileArchive = await client.Containers.GetArchiveFromContainerAsync(container.Id, new()
            {
                Path = filePath
            }, false, token);
            await using Stream containerStream = fileArchive.Stream;
            MemoryStream resultStream = new();
            await using TarReader tar = new(containerStream);

            TarEntry? entry;
            while ((entry = await tar.GetNextEntryAsync(true, token)) != null)
            {
                if (entry.EntryType != TarEntryType.Directory)
                {
                    await entry.DataStream!.CopyToAsync(resultStream, token);
                    return resultStream;
                }
            }
            await resultStream.DisposeAsync();
            return null;
        }
        /// <summary>
        /// Gets the file stat from container.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="throwEx">Throw docker exceptions?</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>An instance of <see cref="ContainerPathStatResponse"/>.</returns>
        public async Task<ContainerPathStatResponse?> GetFileStatFromContainerAsync(ContainerInfo container, string filePath, bool throwEx = false, CancellationToken token = default)
        {
            try
            {
                GetArchiveFromContainerResponse fileArchive = await client.Containers.GetArchiveFromContainerAsync(container.Id, new()
                {
                    Path = filePath
                }, true, token);
                return fileArchive.Stat;
            }
            catch (DockerContainerNotFoundException)
            {
                if (throwEx)
                {
                    throw;
                }
            }
            return null;
        }
        /// <summary>
        /// Starts container.
        /// </summary>
        /// <param name="container">The container info.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns></returns>
        public async Task StartContainerAsync(ContainerInfo container, CancellationToken token = default)
        {
            await client.Containers.StartContainerAsync(container.Id, new()
            {
                DetachKeys = string.Empty,
            }, token);
        }
        /// <summary>
        /// Stops container.
        /// </summary>
        /// <param name="container">The container info.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns></returns>
        public async Task StopContainerAsync(ContainerInfo container, CancellationToken token = default)
        {
            await client.Containers.StopContainerAsync(container.Id, new()
            {
                WaitBeforeKillSeconds = 0,
            }, token);
        }
        /// <summary>
        /// Restarts container.
        /// </summary>
        /// <param name="container">The container info.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns></returns>
        public async Task RestartContainerAsync(ContainerInfo container, CancellationToken token = default)
        {
            await client.Containers.RestartContainerAsync(container.Id, new()
            {
                WaitBeforeKillSeconds = 0
            }, token);
        }
        /// <summary>
        /// Checks container exists.
        /// </summary>
        /// <param name="container">The container info.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns><c>true</c> if container exists; otherwise <c>false</c>.</returns>
        public async Task<bool> ContainerExistsAsync(ContainerInfo container, CancellationToken token = default)
        {
            return (await GetAllContainersAsync(true, token)).FirstOrDefault(c => container.IsEqual([.. c.Names, c.ID])) != default;
        }
        /// <inheritdoc/>
        public void Dispose()
        {
            client.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
