namespace Company.Product.Features.RenameItem;

/// <summary>Renames an item; <paramref name="Version"/> is the version the client read.</summary>
public sealed record RenameItemCommand(Guid Id, string Name, int Version);
