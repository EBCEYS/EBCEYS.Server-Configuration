using System.ComponentModel.DataAnnotations;
using EBCEYS.ContainersEnvironment.Configuration.Models;
using EBCEYS.Server_configuration.Middle;
using Microsoft.AspNetCore.Mvc;

namespace EBCEYS.Server_configuration.Controllers
{
    /// <summary>
    /// Controller wich manipulates the config files.
    /// </summary>
    [Route("/api/[controller]")]
    [ApiController]
    public class ConfigurationController(ILogger<ConfigurationController> logger, ConfigurationProcessingService configs) : ControllerBase
    {
        /// <summary>
        /// Gets the configuration info for <paramref name="containerTypeName"/>.
        /// </summary>
        /// <param name="containerTypeName">The container type name.</param>
        /// <param name="containerSavePath">The container save path.</param>
        /// <response code="200">Configuration file info for <paramref name="containerTypeName"/>.</response>
        /// <response code="204">No config files for <paramref name="containerTypeName"/>.</response>
        /// <response code="400">Incorrect query params.</response>
        /// <response code="500">Internal error.</response>
        /// <returns></returns>
        [HttpGet("files/info")]
        [ProducesResponseType<IEnumerable<ConfigurationFileInfo>>(StatusCodes.Status200OK, "application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<string>(StatusCodes.Status400BadRequest, "application/text")]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetConfigurationFilesInfo([Required][FromQuery] string containerTypeName, [Required][FromQuery] string containerSavePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(containerTypeName) || string.IsNullOrWhiteSpace(containerSavePath))
                {
                    return BadRequest("Incorrect query params!");
                }

                containerTypeName = Uri.UnescapeDataString(containerTypeName);
                containerSavePath = Uri.UnescapeDataString(containerSavePath);

                logger.LogInformation("Get request from {host} for config {typeName}", Request.HttpContext.Connection.RemoteIpAddress, containerTypeName);
                IEnumerable<ConfigurationFileInfo> confFileInfos = configs.GetConfigInfoForContainer(containerTypeName, containerSavePath);
                if (confFileInfos.Any())
                {
                    logger.LogDebug("Find configs for container {typeNam} {@configs}", containerTypeName, confFileInfos);
                    return Ok(confFileInfos);
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on getting configuration files info! {typeName}", containerTypeName);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
        /// <summary>
        /// Gets the configuration file if exists.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <response code="200">The configuration file.</response>
        /// <response code="204">Configuration file not found.</response>
        /// <response code="400">Incorrect file path param.</response>
        /// <response code="500">Internal error.</response>
        /// <returns></returns>
        [HttpGet("files/{filePath}")]
        [ProducesResponseType<FileStream>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetConfigurationFile([Required][FromRoute] string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return BadRequest($"Param {nameof(filePath)} is incorrect!");
            }

            filePath = Uri.UnescapeDataString(filePath);
            logger.LogInformation("Get configuration file {path} request from {ip}", filePath, Request.HttpContext.Connection.RemoteIpAddress);
            try
            {
                Stream? file = await configs.GetConfigurationFile(filePath);
                if (file == null)
                {
                    logger.LogDebug("File {filePath} not found in config directory!", filePath);
                    return NoContent();
                }
                file.Seek(0, SeekOrigin.Begin);
                logger.LogDebug("Find file {filePath}", filePath);
                return File(file, $"file/{Path.GetExtension(filePath).Trim('.')}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on getting configuration file {path}", filePath);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
        /// <summary>
        /// Gets the <paramref name="typeName"/> configuration tar archive.<br/>
        /// Or all configs if <paramref name="typeName"/> is empty.
        /// </summary>
        /// <param name="typeName">The container type name.<br/>Empty to download all configs.</param>
        /// <response code="200">The tar archive with configuration.</response>
        /// <response code="204">Configuration not found.</response>
        /// <response code="500">Internal error.</response>
        /// <returns></returns>
        [HttpGet("archive/tar")]
        [ProducesResponseType<Stream>(StatusCodes.Status200OK, "application/x-tar")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCurrentConfiguration([FromQuery] string? typeName = null)
        {
            if (!string.IsNullOrWhiteSpace(typeName))
            {
                typeName = Uri.UnescapeDataString(typeName);
            }
            else
            {
                typeName = null;
            }
            try
            {
                Stream? archive = await configs.GetContainerTypeConfigArchive(typeName);
                if (archive == null)
                {
                    return NoContent();
                }
                archive.Seek(0, SeekOrigin.Begin);
                return File(archive, "application/x-tar", $"{typeName}-configuration.tar");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on getting configuration file archive of {typeName}!", typeName);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
        /// <summary>
        /// Patchs new configuration. The request body size limit is 200 mb.
        /// </summary>
        /// <param name="archiveFile">The tar archive with configuration.</param>
        /// <param name="removeOldFiles">Remove old files.</param>
        /// <response code="200">Successfully patch new configuration.</response>
        /// <response code="400">Incorrect archive format.</response>
        /// <response code="500">Internal error.</response>
        /// <returns></returns>
        [HttpPatch("archive/tar")]
        [ProducesResponseType(200)]
        [ProducesResponseType<string>(400)]
        [ProducesResponseType<string>(500)]
        [RequestSizeLimit(200_000_000)]
        public async Task<IActionResult> PatchNewConfiguration([Required][FromQuery] bool removeOldFiles, IFormFile archiveFile)
        {
            if (Path.GetExtension(archiveFile.FileName) != ".tar")
            {
                return BadRequest("Incorrect file format!");
            }
            try
            {
                await using Stream stream = archiveFile.OpenReadStream();
                if (await configs.PatchConfigs(stream, removeOldFiles))
                {
                    return Ok();
                }
                return BadRequest("Error on processing archive file!");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on patching new configuration!");
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
