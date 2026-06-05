# Skill: Error & Exception Handling Strategy

**Description:** Design and implement consistent error handling across layers. Learn when to throw exceptions, use Result types, translate errors across boundaries, and ensure domain purity.

---

## What This Skill Does

Guides you through:
- **Choosing an error handling approach** for your feature
- **Domain vs. application vs. infrastructure errors** — when to use each
- **Translating exceptions across layer boundaries** — preventing leaks
- **Logging and monitoring** — capturing errors where they matter
- **Testing error scenarios** — ensuring error paths work

---

## The Error Handling Philosophy

### Core Principles

1. **Domain layer is pure** — Business logic has no infrastructure concerns
2. **Errors flow inward** — Infrastructure handles external failures; domain doesn't know about them
3. **One error representation per layer** — Domain exceptions, Result types, HTTP errors
4. **Log once at boundary** — Don't repeat logging across layers
5. **Explicit error handling** — Code should clearly show what can fail

---

## Error Types by Layer

### Domain Layer

**What can go wrong**: Business rule violations

**How to handle**: Domain-specific exceptions

```csharp
// ✓ Domain exception
public sealed class InsufficientBalanceException : DomainException
{
    public decimal Required { get; }
    public decimal Available { get; }
    
    public InsufficientBalanceException(decimal required, decimal available)
        : base($"Balance {available} insufficient for {required}")
    {
        Required = required;
        Available = available;
    }
}

// Usage in domain
public void Withdraw(Money amount)
{
    if (amount.Amount > Balance)
        throw new InsufficientBalanceException(amount.Amount, Balance);
    
    Balance = Balance.Subtract(amount);
}
```

**Key rules:**
- ✓ Derive from `DomainException` base class
- ✓ Include relevant data (required, available, etc.)
- ✓ Throw when business rule is violated
- ✗ Don't catch external API errors (handle in adapter)
- ✗ Don't reference infrastructure types

---

### Application Layer

**What can go wrong**: Command/query failures, validation errors

**How to handle**: Result types or domain exceptions

```csharp
// ✓ Application Result type
public sealed record CommandResult<T>(
    T? Data = default,
    string? Error = null,
    bool Success = true)
{
    public static CommandResult<T> Ok(T data) => new(data, Success: true);
    public static CommandResult<T> Fail(string error) => new(Error: error, Success: false);
}

// Usage in handler
public sealed class CreateOrderHandler : IRequestHandler<CreateOrderCommand, CommandResult<OrderDto>>
{
    private readonly IOrderRepository _orders;
    
    public async Task<CommandResult<OrderDto>> Handle(CreateOrderCommand cmd, CancellationToken ct)
    {
        try
        {
            var order = Order.Create(cmd.CustomerId, cmd.Lines);
            await _orders.SaveAsync(order, ct);
            return CommandResult<OrderDto>.Ok(new OrderDto { Id = order.Id });
        }
        catch (InsufficientBalanceException ex)
        {
            return CommandResult<OrderDto>.Fail($"Order failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            // Log unexpected errors
            _logger.LogError(ex, "CreateOrder failed unexpectedly");
            return CommandResult<OrderDto>.Fail("An unexpected error occurred");
        }
    }
}
```

**Key rules:**
- ✓ Catch domain exceptions; translate to Result
- ✓ Return structured result with status
- ✓ Catch unexpected exceptions; log & return generic error
- ✗ Don't throw infrastructure errors upward
- ✗ Don't let exceptions escape to presentation

---

### Infrastructure Layer (Adapters)

**What can go wrong**: Database errors, API timeouts, file system issues

**How to handle**: Wrap in adapter exceptions or return errors to application

```csharp
// ✓ Adapter pattern: Wrap external errors
public sealed class OrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<OrderRepository> _logger;
    
    public async Task SaveAsync(Order order, CancellationToken ct)
    {
        try
        {
            _context.Orders.Add(_mapper.MapToPersistence(order));
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to save order {OrderId}", order.Id);
            throw new RepositoryException($"Failed to save order: {ex.Message}", ex);
        }
    }
}

// ✓ External API adapter with resilience
public sealed class EmailNotificationAdapter : INotificationSender
{
    private readonly HttpClient _client;
    private readonly IAsyncPolicy<HttpResponseMessage> _policy;
    
    public async Task SendAsync(Notification notification, CancellationToken ct)
    {
        try
        {
            var response = await _policy.ExecuteAsync(
                () => _client.PostAsync("/send", new StringContent(...), ct));
            
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Email service unavailable");
            throw new NotificationServiceException("Email service is unavailable", ex);
        }
    }
}
```

**Key rules:**
- ✓ Catch infrastructure exceptions
- ✓ Wrap in domain-agnostic adapter exceptions
- ✓ Log with context (what was being done?)
- ✓ Include original exception (`throw new X(..., ex)`)
- ✗ Don't throw raw database or HTTP errors

---

### Presentation Layer (Controllers)

**What can go wrong**: Invalid input, unauthorized access, resource not found

**How to handle**: Translate to HTTP responses

```csharp
// ✓ Controller with error translation
[ApiController]
[Route("api/[controller]")]
public sealed class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrdersController> _logger;
    
    [HttpPost]
    public async Task<IActionResult> CreateOrder(CreateOrderDto dto)
    {
        try
        {
            var command = new CreateOrderCommand(dto.CustomerId, dto.Lines);
            var result = await _mediator.Send(command);
            
            return result.Success
                ? Ok(result.Data)
                : BadRequest(new { error = result.Error });
        }
        catch (InsufficientBalanceException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (RepositoryException ex)
        {
            _logger.LogError(ex, "Repository error in CreateOrder");
            return StatusCode(500, new { error = "A server error occurred" });
        }
    }
}
```

**Key rules:**
- ✓ Catch domain exceptions → 400 Bad Request
- ✓ Catch application errors → 422 Unprocessable Entity
- ✓ Catch system errors → 500 Internal Server Error
- ✓ Log server errors (4xx is client, so usually not logged)
- ✗ Don't expose internal error details to client
- ✗ Don't leak stack traces

---

## Error Translation Pattern

### Problem

Errors come from different layers but must be handled consistently.

### Solution: Translation Chain

```
Infrastructure Error     Adapter Exception       Application Result     HTTP Response
    ↓                           ↓                        ↓                   ↓
DbUpdateException  →  RepositoryException  →  CommandResult.Fail  →  500 ISE
HttpRequestException → NotificationException → CommandResult.Fail  →  503 Service Unavailable
ArgumentException    → DomainException        → CommandResult.Fail  →  400 Bad Request
```

### Implementation

```csharp
// 1. Infrastructure raises error
var response = await client.GetAsync("/api/resource");  // Throws HttpRequestException

// 2. Adapter catches and translates
catch (HttpRequestException ex)
{
    throw new ExternalServiceException("Service unavailable", ex);
}

// 3. Application handler catches and translates
catch (ExternalServiceException ex)
{
    return CommandResult<T>.Fail("External service is temporarily unavailable");
}

// 4. Controller catches and translates
catch (Exception ex)
{
    return StatusCode(503, new { error = ex.Message });
}
```

---

## Logging Strategy

### Log at Boundaries Only

```csharp
// ✓ Log at entry point (once)
public async Task<IActionResult> CreateOrder(CreateOrderDto dto)
{
    _logger.LogInformation("Creating order for customer {CustomerId}", dto.CustomerId);
    
    try
    {
        var result = await _mediator.Send(new CreateOrderCommand(...));
        
        if (result.Success)
            _logger.LogInformation("Order created: {OrderId}", result.Data.Id);
        else
            _logger.LogWarning("Order creation failed: {Error}", result.Error);
        
        return result.Success ? Ok(result.Data) : BadRequest(result.Error);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "CreateOrder failed unexpectedly");
        return StatusCode(500, new { error = "Server error" });
    }
}

// ✗ Don't log in intermediate layers
public class CreateOrderHandler : IRequestHandler<...>
{
    public async Task<CommandResult<OrderDto>> Handle(CreateOrderCommand cmd, CancellationToken ct)
    {
        // Don't log here — let controller log instead
        // This keeps logging concerns out of business logic
        
        try
        {
            var order = Order.Create(cmd.CustomerId, cmd.Lines);
            await _orders.SaveAsync(order, ct);
            return CommandResult<OrderDto>.Ok(...);
        }
        catch (InsufficientBalanceException ex)
        {
            return CommandResult<OrderDto>.Fail(ex.Message);
        }
    }
}
```

**Key rules:**
- ✓ Log at HTTP controller (request entry point)
- ✓ Log at adapter boundaries (external calls)
- ✓ Include structured context (customer ID, order ID, etc.)
- ✗ Don't log in domain layer
- ✗ Don't log duplicate messages
- ✗ Don't log sensitive data (passwords, tokens)

---

## Testing Error Scenarios

### Domain Errors (Unit Tests)

```csharp
[Fact]
public void Withdraw_InsufficientBalance_ThrowsException()
{
    var account = new Account(Money.Create(100, "USD"));
    var withdrawal = Money.Create(150, "USD");
    
    var ex = Assert.Throws<InsufficientBalanceException>(
        () => account.Withdraw(withdrawal));
    
    Assert.Equal(100, ex.Available);
    Assert.Equal(150, ex.Required);
}
```

### Application Errors (Handler Tests with Fakes)

```csharp
[Fact]
public async Task CreateOrder_InsufficientFunds_ReturnsFailure()
{
    var handler = new CreateOrderHandler(_fakeOrderRepo);
    var order = new CreateOrderCommand(CustomerId.Default, /* insufficient lines */);
    
    var result = await handler.Handle(order, CancellationToken.None);
    
    Assert.False(result.Success);
    Assert.Contains("insufficient", result.Error);
}
```

### Infrastructure Errors (Integration Tests)

```csharp
[Fact]
public async Task SaveOrder_DatabaseDown_ThrowsRepositoryException()
{
    var repo = new OrderRepository(_fakeDbContext);
    _fakeDbContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
        .ThrowsAsync(new DbUpdateException("Connection failed"));
    
    var ex = await Assert.ThrowsAsync<RepositoryException>(
        () => repo.SaveAsync(new Order(), CancellationToken.None));
    
    Assert.IsType<DbUpdateException>(ex.InnerException);
}
```

---

## Decision Tree

**When to throw?**

```
Is this a business rule violation?
├─ YES → Throw DomainException
└─ NO → Is this an expected error in a handler?
        ├─ YES → Return CommandResult.Fail(error)
        └─ NO → Is this an infrastructure failure?
                ├─ YES → Wrap in adapter exception
                └─ NO → Let it bubble; log at controller
```

---

## Common Patterns

### Pattern 1: Guard Clauses in Domain

```csharp
public sealed class Order
{
    public static Order Create(Guid customerId, IEnumerable<OrderLine> lines)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("Customer ID required", nameof(customerId));
        
        var lineList = lines?.ToList() ?? [];
        if (lineList.Count == 0)
            throw new DomainException("Order must have at least one line");
        
        return new Order(customerId, lineList);
    }
}
```

### Pattern 2: Try-Catch with Translation

```csharp
public async Task<IActionResult> GetOrder(Guid id)
{
    try
    {
        var order = await _mediator.Send(new GetOrderQuery(id));
        return Ok(order);
    }
    catch (OrderNotFoundException ex)
    {
        return NotFound(new { error = ex.Message });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "GetOrder failed");
        return StatusCode(500, new { error = "Server error" });
    }
}
```

### Pattern 3: Resilience with Fallback

```csharp
// Using Polly for resilience
var policy = Policy
    .Handle<HttpRequestException>()
    .Or<TimeoutException>()
    .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
    .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: attempt => 
            TimeSpan.FromSeconds(Math.Pow(2, attempt)));

var response = await policy.ExecuteAsync(
    () => _client.GetAsync(url));
```

---

## Integration with Other Skills

### With code-review

Review finds inconsistent error handling → Use this skill to standardize

### With adr-creation

Document error handling decisions → Create ADR for consistency

### With gap-analysis

Identify projects with mixed error patterns → Use this skill to audit

---

## Quick Reference

| Scenario | Layer | Approach | Example |
|----------|-------|----------|---------|
| Invalid input | Domain | Throw DomainException | Order has no lines |
| Business rule violated | Domain | Throw DomainException | Insufficient balance |
| Command fails | Application | Return Result.Fail | Aggregate not found |
| Database error | Infrastructure | Wrap in RepositoryException | Connection timeout |
| API timeout | Infrastructure | Wrap in ServiceException | External API down |
| HTTP request | Presentation | Translate to status code | 400, 500, etc. |

---

## Tips

- **Be explicit**: Throw/catch named exceptions, not generic Exception
- **Include context**: Pass relevant data to exceptions (required amount, actual amount, etc.)
- **Test error paths**: Don't just test the happy path
- **Wrap at boundaries**: Translate errors where layers meet
- **Log at edges**: Log at HTTP and external call boundaries
- **Keep domain pure**: Domain code throws domain exceptions only
- **Document in code**: Add comments for non-obvious error handling
