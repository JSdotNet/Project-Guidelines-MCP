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

    /// <summary>
    /// Concurrency token (ADR 0006): every change increments it, the client sends back the value it
    /// read, and a stale value becomes <see cref="Conflict"/>.
    /// </summary>
    public int Version { get; private set; }

    public static Result<Item> Create(string name, DateTimeOffset now) =>
        Validate(name) is { } invalid
            ? invalid
            : new Ok<Item>(new Item(Guid.CreateVersion7(now), name.Trim(), now));

    /// <summary>Renames the item when <paramref name="expectedVersion"/> is the version it holds.</summary>
    public Result<Item> Rename(string name, int expectedVersion)
    {
        if (expectedVersion != Version)
        {
            return new Conflict();
        }

        if (Validate(name) is { } invalid)
        {
            return invalid;
        }

        Name = name.Trim();
        Version++;
        return new Ok<Item>(this);
    }

    public ItemDto ToDto() => new(Id, Name, CreatedAt, Version);

    private static Invalid? Validate(string name) =>
        string.IsNullOrWhiteSpace(name) || name.Length > MaxNameLength
            ? new Invalid(new Dictionary<string, string[]>
            {
                [nameof(Name)] = [$"Name is required and at most {MaxNameLength} characters."],
            })
            : null;
}
