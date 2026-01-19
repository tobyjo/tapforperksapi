# TapForPerksAPI.Tests

Unit test project for TapForPerksAPI using xUnit, Moq, and FluentAssertions.

## Test Coverage

### Controller Tests (`Controllers/RewardOwnerScanControllerTests.cs`)
Tests for `RewardOwnerScanController` - HTTP layer testing with mocked service dependencies.

**Tests Included:**
- ? `GetScanEventForReward_WhenEventExists_ReturnsOkWithDto`
- ? `GetScanEventForReward_WhenEventNotFound_ReturnsNotFound`
- ? `GetUserBalanceForReward_WhenUserExists_ReturnsOkWithBalance`
- ? `GetUserBalanceForReward_WhenUserNotFound_ReturnsNotFound`
- ? `CreatePointsAndClaimRewards_WhenValid_ReturnsCreatedAtRoute`
- ? `CreatePointsAndClaimRewards_WithRewardsClaimed_ReturnsSuccessWithClaimedInfo`
- ? `CreatePointsAndClaimRewards_WhenServiceFails_ReturnsBadRequest`
- ? `CreatePointsAndClaimRewards_WithInsufficientPoints_ReturnsBadRequest`
- ? `Constructor_WithNullRewardService_ThrowsArgumentNullException`

**Total: 9 tests**

### Service Tests (`Services/RewardServiceTests.cs`)
Tests for `RewardService` - Business logic testing with mocked repository and mapper.

**Tests Included:**
- ? `GetScanEventForRewardAsync_WhenEventExists_ReturnsSuccess`
- ? `GetScanEventForRewardAsync_WhenEventNotFound_ReturnsFailure`
- ? `GetUserBalanceForRewardAsync_WhenUserAndRewardExist_ReturnsBalance`
- ? `GetUserBalanceForRewardAsync_WhenUserNotFound_ReturnsFailure`
- ? `GetUserBalanceForRewardAsync_WhenNoBalanceExists_ReturnsZeroBalance`
- ? `ProcessScanAndRewardsAsync_WithValidRequest_CreatesBalanceAndScanEvent`
- ? `ProcessScanAndRewardsAsync_WithRewardClaim_DeductsPointsAndCreatesRedemptions`
- ? `ProcessScanAndRewardsAsync_WithInsufficientPoints_ReturnsFailure`

**Total: 8 tests**

## Running Tests

### Run all tests:
```sh
dotnet test
```

### Run with detailed output:
```sh
dotnet test --logger "console;verbosity=detailed"
```

### Run tests with coverage:
```sh
dotnet test --collect:"XPlat Code Coverage"
```

### Run specific test class:
```sh
dotnet test --filter FullyQualifiedName~RewardOwnerScanControllerTests
```

### Run specific test method:
```sh
dotnet test --filter FullyQualifiedName~CreatePointsAndClaimRewards_WhenValid_ReturnsCreatedAtRoute
```

## Test Dependencies

- **xUnit** (v2.9.2) - Test framework
- **Moq** (v4.20.72) - Mocking framework
- **FluentAssertions** (v8.8.0) - Fluent assertion library
- **Microsoft.EntityFrameworkCore.InMemory** (v10.0.2) - For repository testing (future use)

## Test Architecture

### Controller Layer Tests
- Mock `IRewardService`
- Test HTTP responses (200, 201, 400, 404)
- Verify routing and CreatedAtRoute behavior
- Test null parameter validation

### Service Layer Tests  
- Mock `ISaveForPerksRepository` and `IMapper`
- Test business logic (validation, calculations, transactions)
- Verify repository method calls
- Test success/failure paths using Result pattern

### Future Tests
- Repository integration tests using InMemory database
- End-to-end integration tests
- Performance tests

## Test Conventions

### Naming Convention:
```
MethodName_StateUnderTest_ExpectedBehavior
```

Examples:
- `GetScanEventForReward_WhenEventExists_ReturnsOkWithDto`
- `CreatePointsAndClaimRewards_WithInsufficientPoints_ReturnsBadRequest`

### AAA Pattern:
All tests follow the Arrange-Act-Assert pattern:
```csharp
[Fact]
public async Task TestMethod()
{
    // Arrange - Set up test data and mocks
    
    // Act - Execute the method under test
    
    // Assert - Verify the results
}
```

## Test Results

```
Test Run Successful.
Total tests: 17
     Passed: 17
 Total time: 5.9s
```

## Adding New Tests

1. Create test class in appropriate folder (`Controllers/`, `Services/`, or `Repositories/`)
2. Follow naming conventions
3. Use Moq for mocking dependencies
4. Use FluentAssertions for assertions
5. Follow AAA pattern
6. Run `dotnet test` to verify

## CI/CD Integration

These tests are designed to run in CI/CD pipelines:

```yaml
# Example GitHub Actions
- name: Run tests
  run: dotnet test --logger "trx" --results-directory "TestResults"
  
- name: Publish test results
  uses: actions/upload-artifact@v2
  with:
    name: test-results
    path: TestResults
```

## Code Coverage

To generate code coverage report:
```sh
dotnet test --collect:"XPlat Code Coverage"
```

Install reportgenerator:
```sh
dotnet tool install -g dotnet-reportgenerator-globaltool
```

Generate HTML report:
```sh
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
```

## Best Practices

? **One assertion per test** - Keep tests focused  
? **Mock external dependencies** - Tests should be isolated  
? **Test both success and failure paths** - Comprehensive coverage  
? **Use meaningful test names** - Self-documenting tests  
? **Keep tests fast** - No real database or network calls  
? **Use FluentAssertions** - Readable assertions with better error messages  

## Troubleshooting

### Tests fail to discover:
```sh
dotnet clean
dotnet build
dotnet test
```

### Moq setup not working:
- Verify interface methods match exactly
- Use `It.IsAny<T>()` for flexible parameter matching
- Use `.Verifiable()` and `.Verify()` to ensure mocks are called

### FluentAssertions errors:
- Ensure using `FluentAssertions` namespace
- Check method chaining (`.Should().BeOfType<T>().Subject`)

## Contributing

When adding new features to TapForPerksAPI:
1. Write tests first (TDD approach)
2. Ensure all existing tests pass
3. Add tests for new functionality
4. Aim for >80% code coverage on business logic
