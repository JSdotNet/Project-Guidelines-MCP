using System.ComponentModel.DataAnnotations;

namespace Company.Product.Abstractions.DataTransferObjects;

/// <summary>Renames an item; <see cref="Version"/> is the version the client read (ADR 0006).</summary>
public sealed record RenameItemRequest(
    [property: Required, StringLength(CreateItemRequest.MaxNameLength)] string Name,
    int Version);
