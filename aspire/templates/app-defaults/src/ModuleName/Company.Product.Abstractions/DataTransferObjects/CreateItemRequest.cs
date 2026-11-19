using System.ComponentModel.DataAnnotations;

namespace Company.Product.Abstractions.DataTransferObjects;

public sealed record CreateItemRequest([property: Required, StringLength(CreateItemRequest.MaxNameLength)] string Name)
{
    public const int MaxNameLength = 200;
}
