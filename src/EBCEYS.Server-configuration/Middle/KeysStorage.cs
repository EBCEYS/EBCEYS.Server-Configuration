using System.Collections.Concurrent;
using System.Formats.Tar;
using System.Text.Json;
using EBCEYS.Server_configuration.Middle.Archives;
using EBCEYS.Server_configuration.Options;

namespace EBCEYS.Server_configuration.Middle
{
    /// <summary>
    /// A <see cref="KeysStorageService"/> class.
    /// </summary>
    public class KeysStorageService(ILogger<KeysStorageService> logger, IArchiveHelper archive, KeysStorageOptions? opts = null) : BackgroundService
    {
        private readonly KeysStorageOptions opts = opts ?? KeysStorageOptions.CreateFromEnvironment();
        /// <summary>
        /// Key - key identity.<br/>
        /// Value - key value.
        /// </summary>
        private readonly ConcurrentDictionary<string, string> keys = [];
        private readonly ConcurrentDictionary<string, KeyFileInfo> keyFilesLastUpdate = [];

        private bool IsNowUpdating = false;

        /// <summary>
        /// Gets the saved keys.
        /// </summary>
        /// <returns></returns>
        public IReadOnlyDictionary<string, string> GetKeys()
        {
            return keys;
        }
        /// <summary>
        /// TEMP METHOD REMOVE LATER
        /// </summary>
        /// <returns></returns>
        public string GetRandomKeyFilePath()
        {
            return keyFilesLastUpdate.Where(f => f.Value != default).Select(f => f.Value.FileName).ElementAt(Random.Shared.Next(0, keyFilesLastUpdate.Count));
        }

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!Directory.Exists(opts.KeysDirPath))
            {
                logger.LogWarning("Not found keys directory! Create {dir}", opts.KeysDirPath);
            }
            DirectoryInfo keysDir = Directory.CreateDirectory(opts.KeysDirPath);

#if DEBUG
            DirectoryInfo testSubDir = keysDir.CreateSubdirectory("somekey_directory");
            File.WriteAllText(Path.Combine(testSubDir.FullName, "somekey_file.key"), "{\"SomeKey\":\"SomeKeyValue\"}");
#endif

            while (!stoppingToken.IsCancellationRequested)
            {
                if (IsNowUpdating)
                {
                    await Task.Delay(opts.CheckKeyFilesPeriod, stoppingToken);
                    continue;
                }
                try
                {
                    await SetKeysAsync(keysDir, stoppingToken);
                    if (opts.ForgetOldKeys)
                    {
                        ForgetOldKeys();
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error on keys storage processing!");
                }
                await Task.Delay(opts.CheckKeyFilesPeriod, stoppingToken);
            }
        }

        private void ForgetOldKeys()
        {
            foreach ((string key, FileInfo fileInfo) in keyFilesLastUpdate.Select(x => (x.Key, new FileInfo(x.Value.FileName))).ToArray())
            {
                if (!fileInfo.Exists)
                {
                    logger.LogInformation("Key file {path} does not exists. Remove from cache.", fileInfo.FullName);
                    keyFilesLastUpdate.TryRemove(key, out _);
                    keys.TryRemove(key, out _);
                }
            }
        }

        private async Task SetKeysAsync(DirectoryInfo keysDir, CancellationToken stoppingToken = default)
        {
            IEnumerable<FileInfo> keyFiles = keysDir.EnumerateFiles("*.key", SearchOption.AllDirectories);
            foreach (FileInfo file in keyFiles)
            {
                DateTime lastWrite = file.LastWriteTimeUtc;
                KeyFileInfo? keyFileInfo = keyFilesLastUpdate.Values.FirstOrDefault(k => k.FileName == file.FullName);
                if (keyFileInfo != default && lastWrite <= keyFileInfo.LastWriteUTC)
                {
                    continue;
                }
                try
                {
                    Dictionary<string, string>? fileKeys = JsonSerializer.Deserialize<Dictionary<string, string>>(await File.ReadAllBytesAsync(file.FullName, stoppingToken));
                    if (fileKeys == null)
                    {
                        logger.LogWarning("Empty json file? {file}", file.FullName);
                        continue;
                    }
                    foreach (KeyValuePair<string, string> key in fileKeys) 
                    {
                        string formatedKeyKey = FormatKeyKey(keysDir.FullName, file.FullName, key.Key);
                        if (keys.ContainsKey(formatedKeyKey))
                        {
                            logger.LogWarning("Overwriting key {key}", formatedKeyKey);
                        }
                        keys[formatedKeyKey] = key.Value;
                        keyFilesLastUpdate[formatedKeyKey] = new(file.FullName, lastWrite);
                        logger.LogInformation("Save {key} to memory. Last file update(UTC) {lastWrite}", formatedKeyKey, lastWrite);
                    }
                }
                catch (JsonException ex)
                {
                    logger.LogError(ex, "Error on deserializing key file {file}", file.FullName);
                }
            }
        }

        private static string FormatKeyKey(string keysDirPath, string fileName, string key)
        {
            return $"<<{Path.ChangeExtension(fileName, null).Replace(keysDirPath, "").Trim(Path.DirectorySeparatorChar).Replace(Path.DirectorySeparatorChar, '.')}.{key}>>";
        }
        internal async Task<Stream?> GetKeyFilesArchive()
        {
            if (IsNowUpdating)
            {
                return null;
            }
            return await archive.ArchivateDirectoryAsync(opts.KeysDirPath, false);
        }
        internal async Task<bool> PatchKeys(Stream tarArchive, bool removeOldFiles)
        {
            IsNowUpdating = true;
            try
            {
                DirectoryInfo keysDir = new(opts.KeysDirPath);
                if (!keysDir.Exists) { keysDir.Create(); }
                DirectoryInfo tmpKeysDir = keysDir.Parent?.CreateSubdirectory("tmpKeys") ?? Directory.CreateDirectory(Path.Combine("/", "tmpKeys"));
                try
                {
                    await archive.ExtractArchiveAsync(tarArchive, tmpKeysDir, true);
                    if (removeOldFiles)
                    {
                        keysDir.Delete(true);
                        tmpKeysDir.MoveTo(keysDir.FullName);
                    }
                    else
                    {

                        CopyFilesToNewDir(tmpKeysDir, keysDir, true);
                        tmpKeysDir.Delete(true);
                    }
                }
                catch (Exception)
                {
                    tmpKeysDir.Delete(true);
                    throw;
                }
                await SetKeysAsync(keysDir);
                IsNowUpdating = false;
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on extracting archive!");
                IsNowUpdating = false;
                return false;
            }
        }

        private static void CopyFilesToNewDir(DirectoryInfo orig, DirectoryInfo dest, bool overwrite)
        {
            if (!orig.Exists)
            {
                return;
            }
            dest.Create();
            foreach (FileInfo file in orig.EnumerateFiles("*", SearchOption.TopDirectoryOnly))
            {
                string newPath = Path.Combine(dest.FullName, file.Name);
                file.CopyTo(newPath, overwrite);
            }
            foreach (DirectoryInfo dir in orig.EnumerateDirectories("*", SearchOption.TopDirectoryOnly))
            {
                DirectoryInfo newDestDir = new(Path.Combine(dest.FullName, dir.Name));
                CopyFilesToNewDir(dir, newDestDir, overwrite);
            }
        }

        private class KeyFileInfo(string fileName, DateTime lastWriteUTC)
        {
            public string FileName { get; set; } = fileName;
            public DateTime LastWriteUTC { get; set; } = lastWriteUTC;
        }
    }
}
