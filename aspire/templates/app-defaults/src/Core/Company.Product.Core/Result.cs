namespace Company.Product.Core;

/// <summary>Every expected outcome of a use case; match it with an exhaustive switch (ADR 0005).</summary>
public union Result<T>(Ok<T>, NotFound, Invalid, Conflict);

public sealed record Ok<T>(T Value);

public sealed record NotFound;

public sealed record Invalid(IDictionary<string, string[]> Errors);

public sealed record Conflict;
