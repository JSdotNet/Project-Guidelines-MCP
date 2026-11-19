namespace Company.Product.Abstractions.DataTransferObjects;

public sealed record ItemDto(Guid Id, string Name, DateTimeOffset CreatedAt, int Version);
