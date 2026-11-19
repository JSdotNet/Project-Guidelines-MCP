namespace Company.Product.Core;

/// <summary>Handles one command; called directly by its endpoint, no mediator (ADR 0005).</summary>
public interface ICommandHandler<in TCommand, TResult>
{
    Task<Result<TResult>> Handle(TCommand command, CancellationToken cancellationToken);
}

/// <summary>Handles one query; called directly by its endpoint, no mediator (ADR 0005).</summary>
public interface IQueryHandler<in TQuery, TResult>
{
    Task<Result<TResult>> Handle(TQuery query, CancellationToken cancellationToken);
}
