using System.Diagnostics.CodeAnalysis;

namespace EBCEYS.Server_configuration.Middle.Models;

/// <summary>
///     A <see cref="ContainerInfo" /> class.
/// </summary>
/// <remarks>
///     Initiates a new instance of <see cref="ContainerInfo" />.
/// </remarks>
/// <param name="id">The container id or name.</param>
public readonly struct ContainerInfo(string id)
{
    /// <summary>
    ///     The unknown container id.
    /// </summary>
    public const string UnknownContaierId = "Unknown";

    /// <summary>
    ///     The container id or name.
    /// </summary>
    public readonly string Id = id ?? UnknownContaierId;

    /// <summary>
    ///     The default realization of <see cref="ContainerInfo" />.
    /// </summary>
    public static ContainerInfo Default { get; } = new(UnknownContaierId);

    /// <summary>
    ///     Checks is <see cref="Id" /> is equal any <paramref name="anotherId" /> element.
    /// </summary>
    /// <param name="anotherId">The another ids.</param>
    /// <returns><c>true</c> if any of <paramref name="anotherId" /> is equal <see cref="Id" />; otherwise <c>false</c>.</returns>
    public bool IsEqual(params string[] anotherId)
    {
        foreach (var id in anotherId)
            if (string.Compare(Id, id, true) == 0)
                return true;

        return false;
    }

    /// <inheritdoc />
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is ContainerInfo val) return Id == val.Id;
        return false;
    }

    /// <summary>
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator ==(ContainerInfo left, ContainerInfo right)
    {
        return left.Id == right.Id;
    }

    /// <summary>
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <returns></returns>
    public static bool operator !=(ContainerInfo left, ContainerInfo right)
    {
        return !(left == right);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}