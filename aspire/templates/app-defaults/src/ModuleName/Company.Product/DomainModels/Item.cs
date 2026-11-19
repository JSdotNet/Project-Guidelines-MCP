using Company.Product.Abstractions.DataTransferObjects;
using Company.Product.Core;

namespace Company.Product.DomainModels;

/// <summary>The module's sample aggregate; replace it with the first real one.</summary>
public sealed class Item
{
    public const int MaxNameLength = CreateItemRequest.MaxNameLength;

    private Item(Guid id, string name, DateTimeOffset createdAt)
    {
        Id = id;
        Name = name;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Concurrency token: a stale write becomes <see cref="Conflict"/>.</summary>
    public int Version { get; private set; }

    public static Result<Item> Create(string name, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > MaxNameLength)
        {
            return new Invalid(new Dictionary<string, string[]>
            {
                [nameof(Name)] = [$"Name is required and at most {MaxNameLength} characters."],
            });
        }

        return new Ok<Item>(new Item(Guid.CreateVersion7(now), name.Trim(), now));
    }

    public ItemDto ToDto() => new(Id, Name, CreatedAt, Version);
}
