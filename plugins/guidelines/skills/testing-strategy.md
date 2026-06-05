# Skill: Testing Strategy & Coverage

**Description:** Design effective tests across layers (unit, integration, contract). Organize tests for maintainability, achieve meaningful coverage, and ensure critical paths are validated.

---

## What This Skill Does

Guides you through:
- **Organizing tests by layer** — Domain, application, infrastructure
- **Choosing the right test type** — Unit, integration, end-to-end
- **Mocking and faking** — When to use each
- **Coverage strategy** — What to measure and when
- **Test naming and structure** — Making tests readable
- **Testing error paths** — Not just happy paths

---

## The Testing Philosophy

### Core Principles

1. **Test one thing per test** — One assertion per conceptual rule
2. **Domain tests are pure** — No mocks, no dependencies
3. **Application tests use fakes** — Isolated from infrastructure
4. **Infrastructure tests are integrated** — Use real databases/APIs when possible
5. **Coverage is a guide, not a goal** — Measure meaningful paths, not lines

---

## Test Organization by Layer

### Domain Layer: Pure Unit Tests

**What to test**: Business logic, invariants, calculations

**How to test**: No mocks, no dependencies — just arrange, act, assert

```csharp
// ✓ Domain test: Pure, isolated
[Fact]
public void Money_Create_ValidAmount_Succeeds()
{
    var money = Money.Create(100, "USD");
    
    Assert.Equal(100, money.Amount);
    Assert.Equal("USD", money.Currency);
}

[Fact]
public void Money_Create_NegativeAmount_Throws()
{
    var ex = Assert.Throws<ArgumentException>(
        () => Money.Create(-50, "USD"));
    
    Assert.Contains("Amount must be positive", ex.Message);
}

[Theory]
[InlineData(100, 50, 50)]
[InlineData(100, 100, 0)]
[InlineData(50.50, 25.25, 25.25)]
public void Money_Subtract_VariousAmounts_Succeeds(
    decimal start, decimal subtract, decimal expected)
{
    var money = Money.Create(start, "USD");
    var result = money.Subtract(Money.Create(subtract, "USD"));
    
    Assert.Equal(expected, result.Amount);
}

// ✗ Don't mock domain entities
[Fact]
public void Order_Add_Line_MockingLineWrong()
{
    var mockLine = new Mock<OrderLine>();  // ❌ Don't mock domain objects
    // ...
}
```

**Key patterns:**
- ✓ Test every business rule
- ✓ Test edge cases (0, negative, max values)
- ✓ Use `[Theory]` with `[InlineData]` for multiple cases
- ✓ Test thrown exceptions include useful messages
- ✗ Don't mock domain objects
- ✗ Don't use repositories or databases
- ✗ Don't test getters/setters unless they have logic

**Coverage target**: 100% of domain business logic

---

### Application Layer: Handler Tests with Fakes

**What to test**: Orchestration logic, command/query handling, error cases

**How to test**: Real domain objects, fake ports (repositories, services)

```csharp
// ✓ Application test: With fake repository
public sealed class CreateOrderHandlerTests
{
    private readonly CreateOrderHandler _handler;
    private readonly FakeOrderRepository _fakeOrderRepo;
    private readonly ILogger<CreateOrderHandler> _logger;
    
    public CreateOrderHandlerTests()
    {
        _fakeOrderRepo = new FakeOrderRepository();
        _logger = new Mock<ILogger<CreateOrderHandler>>().Object;
        _handler = new CreateOrderHandler(_fakeOrderRepo, _logger);
    }
    
    [Fact]
    public async Task Handle_ValidCommand_SavesOrderAndReturnsId()
    {
        var cmd = new CreateOrderCommand(
            CustomerId: Guid.NewGuid(),
            Lines: [new OrderLineDto { Sku = "SKU1", Quantity = 2 }]);
        
        var result = await _handler.Handle(cmd, CancellationToken.None);
        
        Assert.True(result.Success);
        Assert.NotNull(result.Data?.Id);
        Assert.Single(await _fakeOrderRepo.GetAllAsync());
    }
    
    [Fact]
    public async Task Handle_InvalidInput_ReturnsFail()
    {
        var cmd = new CreateOrderCommand(
            CustomerId: Guid.Empty,  // ❌ Invalid
            Lines: []);              // ❌ Empty
        
        var result = await _handler.Handle(cmd, CancellationToken.None);
        
        Assert.False(result.Success);
        Assert.Contains("Customer", result.Error);
    }
    
    [Fact]
    public async Task Handle_RepositoryFailure_ReturnsError()
    {
        _fakeOrderRepo.SetupToThrow(
            new RepositoryException("Database unavailable"));
        
        var cmd = new CreateOrderCommand(Guid.NewGuid(), [new OrderLineDto { ... }]);
        var result = await _handler.Handle(cmd, CancellationToken.None);
        
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
    }
}

// ✓ Fake repository implementation
public sealed class FakeOrderRepository : IOrderRepository
{
    private readonly List<Order> _orders = [];
    private Func<Task>? _throwOnSave;
    
    public void SetupToThrow(Exception ex) =>
        _throwOnSave = () => throw ex;
    
    public async Task SaveAsync(Order order, CancellationToken ct)
    {
        if (_throwOnSave != null)
            await _throwOnSave();
        
        _orders.Add(order);
    }
    
    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct) =>
        _orders.FirstOrDefault(o => o.Id == id);
}

// ✗ Don't use real database in app tests
[Fact]
public async Task Handle_RealDatabase_SlowAndUnreliableWrong()
{
    using var db = new ApplicationDbContext(...);  // ❌ Integration test
    var handler = new CreateOrderHandler(new OrderRepository(db));
    // ...
}
```

**Key patterns:**
- ✓ Use fake implementations of ports
- ✓ Test success and failure paths
- ✓ Test error translations
- ✓ Verify state changes (order saved)
- ✗ Don't use real database
- ✗ Don't test infrastructure (that's integration test's job)
- ✗ Don't mock domain objects

**Coverage target**: 80% of application handlers

---

### Infrastructure Layer: Integration Tests

**What to test**: Adapter correctness, error handling, external integrations

**How to test**: Use real infrastructure (containerized DB, test doubles for APIs)

```csharp
// ✓ Integration test: Real database (in container)
public sealed class OrderRepositoryIntegrationTests : IAsyncLifetime
{
    private PostgreSqlContainer _container = null!;
    private ApplicationDbContext _context = null!;
    private OrderRepository _repository = null!;
    
    public async Task InitializeAsync()
    {
        _container = new PostgreSqlBuilder().Build();
        await _container.StartAsync();
        
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;
        
        _context = new ApplicationDbContext(options);
        await _context.Database.MigrateAsync();
        
        _repository = new OrderRepository(_context, new Mapper());
    }
    
    [Fact]
    public async Task SaveAsync_NewOrder_PersistsCorrectly()
    {
        var order = Order.Create(Guid.NewGuid(), [new OrderLine("SKU1", 2)]);
        
        await _repository.SaveAsync(order, CancellationToken.None);
        
        var saved = await _context.Orders.FirstOrDefaultAsync(o => o.Id == order.Id);
        Assert.NotNull(saved);
        Assert.Single(saved.Lines);
    }
    
    [Fact]
    public async Task SaveAsync_DatabaseDown_ThrowsRepositoryException()
    {
        await _container.StopAsync();
        
        var order = Order.Create(Guid.NewGuid(), [new OrderLine("SKU1", 2)]);
        
        var ex = await Assert.ThrowsAsync<RepositoryException>(
            () => _repository.SaveAsync(order, CancellationToken.None));
        
        Assert.IsType<DbUpdateException>(ex.InnerException);
    }
    
    public async Task DisposeAsync()
    {
        _context?.Dispose();
        await _container?.StopAsync()!;
    }
}

// ✓ External API test: Mocked HTTP with test double
[Collection("HttpClientFactory")]
public sealed class EmailServiceAdapterTests
{
    private readonly HttpClient _httpClient;
    private readonly MockHttpMessageHandler _mockHandler;
    
    public EmailServiceAdapterTests()
    {
        _mockHandler = new MockHttpMessageHandler();
        _httpClient = new HttpClient(_mockHandler) 
            { BaseAddress = new Uri("https://email-service.test") };
    }
    
    [Fact]
    public async Task SendAsync_Success_ReturnsNotification()
    {
        _mockHandler.MockResponse(HttpStatusCode.Accepted, new { id = "msg123" });
        
        var adapter = new EmailServiceAdapter(_httpClient, _logger);
        var notification = new Notification("test@example.com", "Subject", "Body");
        
        await adapter.SendAsync(notification, CancellationToken.None);
        
        Assert.True(_mockHandler.RequestWasMade("/send"));
    }
    
    [Fact]
    public async Task SendAsync_ServiceDown_ThrowsServiceException()
    {
        _mockHandler.MockResponse(HttpStatusCode.ServiceUnavailable);
        
        var adapter = new EmailServiceAdapter(_httpClient, _logger);
        var notification = new Notification("test@example.com", "Subject", "Body");
        
        var ex = await Assert.ThrowsAsync<NotificationServiceException>(
            () => adapter.SendAsync(notification, CancellationToken.None));
        
        Assert.NotNull(ex.InnerException);
    }
}
```

**Key patterns:**
- ✓ Use testcontainers for real databases
- ✓ Mock external APIs with test doubles
- ✓ Test both success and error paths
- ✓ Clean up resources properly (IAsyncLifetime)
- ✓ Verify error wrapping (original exception preserved)
- ✗ Don't make real calls to production services
- ✗ Don't mix integration and unit concerns

**Coverage target**: Core adapter flows (success + errors)

---

## Test Naming Convention

Use the `Method_ShouldExpected_WhenCondition` pattern:

```csharp
// ✓ Clear test names
[Fact]
public void Money_Create_Should_Succeed_When_AmountIsPositive() { }

[Fact]
public void Order_Withdraw_Should_ThrowException_When_InsufficientBalance() { }

[Fact]
public async Task CreateOrderHandler_Should_SaveOrder_When_CommandIsValid() { }

// ✗ Poor test names
[Fact]
public void Test1() { }  // ❌ Meaningless

[Fact]
public void CreateOrder() { }  // ❌ Doesn't describe outcome

[Fact]
public void Should_Work() { }  // ❌ Too vague
```

---

## Test Structure: Arrange-Act-Assert

```csharp
[Fact]
public void Method_Expectation_Condition()
{
    // Arrange: Set up test data
    var account = new Account(Money.Create(1000, "USD"));
    var withdrawal = Money.Create(500, "USD");
    
    // Act: Execute the behavior
    account.Withdraw(withdrawal);
    
    // Assert: Verify the outcome
    Assert.Equal(500, account.Balance.Amount);
}
```

---

## Coverage Strategy

### What to Measure

```
Total Lines Covered vs. Total Lines
├─ Domain layer:        Target 100%  (pure logic, no external deps)
├─ Application layer:   Target 80%   (handlers, commands, queries)
├─ Infrastructure:      Target 50%+  (complex adapters only)
└─ Presentation:        Target <50%  (mostly I/O translation)
```

### Meaningful vs. Meaningless Coverage

```csharp
// ✗ Meaningless: Testing auto-property
public class User
{
    public string Name { get; set; }
}

[Fact]
public void User_SetName_Succeeds()
{
    var user = new User { Name = "John" };
    Assert.Equal("John", user.Name);  // ❌ Pointless
}

// ✓ Meaningful: Testing business logic
public class User
{
    private string _name;
    public string Name
    {
        get => _name;
        set => _name = string.IsNullOrWhiteSpace(value) 
            ? throw new ArgumentException("Name required") 
            : value;
    }
}

[Fact]
public void User_SetName_Validation_Succeeds()
{
    var user = new User { Name = "John" };
    Assert.Equal("John", user.Name);
}

[Fact]
public void User_SetName_Empty_Throws()
{
    var ex = Assert.Throws<ArgumentException>(() => new User { Name = "" });
    Assert.Contains("required", ex.Message);
}
```

---

## Testing Error Paths

### Error Path Testing Checklist

- [ ] Domain validation exceptions are thrown
- [ ] Application handlers catch and translate errors
- [ ] Infrastructure adapters wrap external failures
- [ ] Controllers translate to correct HTTP status codes
- [ ] Error details are logged (not exposed to client)
- [ ] Null/empty inputs are handled
- [ ] Boundary conditions (max int, empty collection) are tested

```csharp
[Theory]
[MemberData(nameof(InvalidCommandData))]
public async Task CreateOrderHandler_InvalidInput_ReturnsFail(CreateOrderCommand cmd, string expectedError)
{
    var result = await _handler.Handle(cmd, CancellationToken.None);
    
    Assert.False(result.Success);
    Assert.Contains(expectedError, result.Error);
}

public static IEnumerable<object[]> InvalidCommandData => new[]
{
    new object[] { new CreateOrderCommand(Guid.Empty, []), "Customer required" },
    new object[] { new CreateOrderCommand(Guid.NewGuid(), []), "At least one line required" },
};
```

---

## Common Patterns

### Pattern 1: Theory Tests for Multiple Cases

```csharp
[Theory]
[InlineData(1, 1, 2)]
[InlineData(2, 3, 5)]
[InlineData(0, 5, 5)]
public void Add_VariousInputs_CalculatesCorrectly(int a, int b, int expected)
{
    var result = Calculator.Add(a, b);
    Assert.Equal(expected, result);
}
```

### Pattern 2: FluentAssertions for Complex Objects

```csharp
[Fact]
public void CreateOrderHandler_Should_ReturnCorrectDto()
{
    var result = await _handler.Handle(cmd, CancellationToken.None);
    
    result.Data.Should()
        .NotBeNull()
        .And.BeOfType<OrderDto>()
        .Which.Lines.Should().HaveCount(2);
}
```

### Pattern 3: Fakes vs. Mocks

```csharp
// ✓ Fake: Full working implementation
public sealed class FakeOrderRepository : IOrderRepository
{
    private List<Order> _orders = [];
    public async Task SaveAsync(Order order, CancellationToken ct) => _orders.Add(order);
    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct) 
        => _orders.FirstOrDefault(o => o.Id == id);
}

// ✓ Mock: Tracked calls
var mockRepo = new Mock<IOrderRepository>();
mockRepo.Setup(r => r.SaveAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync((Order o, CancellationToken ct) => { /* track call */ });

// Use fakes for handlers (simpler), mocks for verifying calls (rare)
```

---

## Integration with Other Skills

### With code-review

Code review finds untested paths → Add tests here

### With error-handling

Error handling skill shows error scenarios → Test them all

### With gap-analysis

Gap analysis finds missing test projects → Use this skill to design them

---

## Quick Reference

| Test Type | Where | What | How | Tools |
|-----------|-------|------|-----|-------|
| Unit | Domain | Business logic | No mocks | xUnit, FluentAssertions |
| Unit | Application | Handlers | Fake ports | xUnit, Moq |
| Integration | Infrastructure | Adapters | Real DB/APIs | xUnit, Testcontainers |
| Contract | APIs | Agreements | Test double | xUnit, WireMock |

---

## Tips

- **Test behavior, not implementation** — Don't test private methods
- **One assertion per test** — Unless grouped with FluentAssertions
- **Make tests maintainable** — Use builders for complex test data
- **Run tests locally** — Don't rely only on CI
- **Isolate tests** — Each test should be independent
- **Use data builders** — TestDataBuilder pattern for complex objects
- **Keep tests simple** — If test is hard to understand, code might be too
- **Test the contract** — What matters to callers, not internal details
