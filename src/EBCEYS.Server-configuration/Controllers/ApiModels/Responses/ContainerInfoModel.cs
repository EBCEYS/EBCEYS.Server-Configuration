using Docker.DotNet.Models;

namespace EBCEYS.Server_configuration.Controllers.ApiModels.Responses;

/// <summary>
///     A <see cref="ContainerInfoModel" /> class
/// </summary>
/// <remarks>
///     Initiates a new instance of <see cref="ContainerInfoModel" />.
/// </remarks>
/// <param name="container">The container.</param>
public class ContainerInfoModel(ContainerListResponse container)
{
    /// <summary>
    ///     The container id.
    /// </summary>
    public string Id { get; } = container.ID;

    /// <summary>
    ///     The container names.
    /// </summary>
    public IList<string> Names { get; } = container.Names;

    /// <summary>
    ///     The status.
    /// </summary>
    public string Status { get; } = container.Status;

    /// <summary>
    ///     The state.
    /// </summary>
    public string State { get; } = container.State;

    /// <summary>
    ///     The creation datetime.
    /// </summary>
    public DateTime Created { get; } = container.Created;

    /// <summary>
    ///     The ports.
    /// </summary>
    public IList<Port> Ports { get; } = container.Ports;

    /// <summary>
    ///     The labels.
    /// </summary>
    public IDictionary<string, string> Labels { get; } = container.Labels;

    /// <summary>
    ///     The image info.
    /// </summary>
    public ImageInfoModel ImageInfo { get; } = new(container.Image, container.ImageID);
}

/// <summary>
///     A <see cref="ImageInfoModel" /> class.
/// </summary>
/// <remarks>
///     Initiates a new instance of <see cref="ImageInfoModel" />.
/// </remarks>
/// <param name="image">The image name.</param>
/// <param name="imageId">The image id.</param>
public class ImageInfoModel(string image, string imageId)
{
    /// <summary>
    ///     The image name.
    /// </summary>
    public string Image { get; } = image;

    /// <summary>
    ///     The image id.
    /// </summary>
    public string Id { get; } = imageId;
}