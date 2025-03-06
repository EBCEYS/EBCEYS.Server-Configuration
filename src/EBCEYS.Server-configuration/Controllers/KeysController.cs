using System.ComponentModel.DataAnnotations;
using EBCEYS.Server_configuration.Middle;
using Microsoft.AspNetCore.Mvc;

namespace EBCEYS.Server_configuration.Controllers
{
    /// <summary>
    /// The controller to manipulate keys.
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="keys"></param>
    [Route("/api")]
    [ApiController]
    public class KeysController(ILogger<KeysController> logger, KeysStorageService keys) : ControllerBase
    {
        /// <summary>
        /// Patchs new keys. The request body size limit is 200 mb.
        /// </summary>
        /// <param name="archiveFile">The tar archive with keys.</param>
        /// <param name="removeOldFiles">Remove old keys.</param>
        /// <response code="200">Successfully patch new keys.</response>
        /// <response code="400">Incorrect archive format.</response>
        /// <response code="500">Internal error.</response>
        /// <returns></returns>
        [HttpPatch("[controller]/archive/tar")]
        [ProducesResponseType(200)]
        [ProducesResponseType<string>(400)]
        [ProducesResponseType<string>(500)]
        [RequestSizeLimit(200_000_000)]
        public async Task<IActionResult> PatchNewKeys([Required][FromQuery] bool removeOldFiles, IFormFile archiveFile)
        {
            if (Path.GetExtension(archiveFile.FileName) != ".tar")
            {
                return BadRequest("Incorrect file format!");
            }
            try
            {
                await using Stream stream = archiveFile.OpenReadStream();
                if (await keys.PatchKeys(stream, removeOldFiles))
                {
                    return Ok();
                }
                return BadRequest("Error on processing archive file!");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on patching new keys...");
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        /// <summary>
        /// Gets the keys tar archive.
        /// </summary>
        /// <response code="200">The tar archive with keys.</response>
        /// <response code="204">Keys not found.</response>
        /// <response code="500">Internal error.</response>
        /// <returns></returns>
        [HttpGet("[controller]/archive/tar")]
        [ProducesResponseType<Stream>(StatusCodes.Status200OK, "application/x-tar")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetKeysFiles()
        {
            try
            {
                Stream? archive = await keys.GetKeyFilesArchive();
                if (archive == null)
                {
                    return NoContent();
                }
                archive.Seek(0, SeekOrigin.Begin);
                return File(archive, "application/x-tar", "keys.tar");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on getting keys!");
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
