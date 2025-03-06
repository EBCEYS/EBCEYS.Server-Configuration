#if DEBUG
using Docker.DotNet;
using EBCEYS.Server_configuration.Controllers.ApiModels.Responses;
using EBCEYS.Server_configuration.Middle;
using EBCEYS.Server_configuration.Middle.Models;
using Microsoft.AspNetCore.Mvc;

namespace EBCEYS.Server_configuration.Controllers
{
    /// <summary>
    /// A <see cref="DockerApiController"/> class.
    /// </summary>
    /// <remarks>
    /// Initiates a new instance of <see cref="DockerApiController"/>.
    /// </remarks>
    /// <param name="logger">The logger.</param>
    /// <param name="docker">The docker controller.</param>
    /// <param name="keys">The keys.</param>
    [ApiController]
    [Route("api/[controller]")]
    public class DockerApiController(ILogger<DockerApiController> logger, DockerController docker, KeysStorageService keys) : ControllerBase
    {
        private ObjectResult InternalError(object? value)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, value);
        }
        /// <summary>
        /// Gets the containers list.
        /// </summary>
        /// <returns></returns>
        [HttpGet("containers/list")]
        [ProducesResponseType<IEnumerable<ContainerInfoModel>>(StatusCodes.Status200OK)]
        [ProducesResponseType<string>(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetContainersList()
        {
            try
            {
                IEnumerable<ContainerInfoModel> containers = (await docker.GetAllContainersAsync(true)).Select(c => new ContainerInfoModel(c));
                return Ok(containers);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error on getting containers list");
                return InternalError(ex.ToString());
            }
        }

        /// <summary>
        /// Gets the keys.
        /// </summary>
        /// <returns></returns>
        [HttpGet("keys/list")]
        [ProducesResponseType<IReadOnlyDictionary<string, string>>(StatusCodes.Status200OK)]
        public IActionResult GetKeys()
        {
            return Ok(keys.GetKeys());
        }
        /// <summary>
        /// Gets the file stats from container.
        /// </summary>
        /// <returns></returns>
        [HttpGet("container/{id}/file/stats/{path}")]
        public async Task<IActionResult> GetFileStats([FromRoute] string id, [FromRoute] string path)
        {
            ContainerInfo container = new(Uri.UnescapeDataString(id));
            string destPath = Uri.UnescapeDataString(path);
            try
            {
                return Ok(await docker.GetFileStatFromContainerAsync(container, destPath));
            }
            catch (DockerContainerNotFoundException)
            {
                return NotFound($"Container or path not found!");
            }
            catch (Exception ex)
            {
                return InternalError(ex.ToString());
            }
        }
        /// <summary>
        /// Gets the file from container.
        /// </summary>
        /// <returns></returns>
        [HttpGet("container/{id}/file/{path}")]
        public async Task<IActionResult> GetFile([FromRoute] string id, [FromRoute] string path)
        {
            ContainerInfo container = new(Uri.UnescapeDataString(id));
            string destPath = Uri.UnescapeDataString(path);
            try
            {
                Stream? file = await docker.GetFileFromContainerAsync(container, destPath);
                if (file == null)
                {
                    return NotFound("File not found!");
                }
                file.Seek(0, SeekOrigin.Begin);
                return File(file, "form/data", true);
            }
            catch (DockerContainerNotFoundException)
            {
                return NotFound($"Container or path not found!");
            }
            catch (Exception ex)
            {
                return InternalError(ex.ToString());
            }
        }
    }
}
#endif