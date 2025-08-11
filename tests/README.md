# RecipeAppApi Tests

This directory contains the unit tests for the RecipeAppApi solution.

## Test Project Structure

- **RecipeAppApi.Tests.csproj**: Main test project file
- **LoggingTests.cs**: Tests for logging configuration
- **PasswordServiceTests.cs**: Comprehensive tests for the PasswordService

## Test Framework

- **xUnit**: Primary testing framework
- **FluentAssertions**: Fluent assertion library for readable test assertions
- **coverlet.collector**: Code coverage collection

## PasswordService Tests

The `PasswordServiceTests` class provides comprehensive coverage for the `PasswordService` implementation, testing both the `HashPassword` and `VerifyPassword` methods.

### Test Categories

#### HashPassword Method Tests
- **Basic Functionality**: Verifies that hashing returns both hash and salt
- **Salt Uniqueness**: Ensures different salts are generated for the same password
- **Output Validation**: Validates that outputs are proper Base64 strings
- **Length Verification**: Confirms correct byte lengths for hash (32 bytes) and salt (32 bytes)
- **Input Handling**: Tests various password lengths, Unicode characters, and special characters
- **Cryptographic Security**: Verifies that salts are cryptographically random
- **Deterministic Behavior**: Ensures consistent hashing for same password + salt combination
- **Null Handling**: Tests proper exception throwing for null inputs
- **Instance Consistency**: Verifies consistent behavior across multiple service instances

#### VerifyPassword Method Tests
- **Correct Verification**: Tests successful password verification with correct credentials
- **Incorrect Password**: Verifies rejection of wrong passwords
- **Incorrect Hash**: Tests rejection when hash is wrong
- **Incorrect Salt**: Tests rejection when salt is wrong
- **Edge Cases**: Handles empty strings and whitespace passwords
- **Null Parameter Handling**: Tests proper exception throwing for null parameters

### Test Coverage

The tests cover:
- ✅ **Happy Path**: Normal operation scenarios
- ✅ **Edge Cases**: Empty strings, whitespace, very long passwords
- ✅ **Security**: Cryptographic randomness, salt uniqueness
- ✅ **Error Handling**: Null parameter validation
- ✅ **Consistency**: Deterministic behavior and cross-instance consistency
- ✅ **Input Validation**: Various password types and formats

### Running the Tests

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity normal

# Run with test names displayed
dotnet test --logger "console;verbosity=detailed"

# Run specific test class
dotnet test --filter "FullyQualifiedName~PasswordServiceTests"

# Run specific test method
dotnet test --filter "FullyQualifiedName~HashPassword_ShouldReturnHashAndSalt"
```

### Test Results

All 27 tests pass successfully, providing comprehensive coverage of the PasswordService functionality.

## Adding New Tests

When adding new tests:

1. Follow the naming convention: `MethodName_Should_ExpectedBehavior`
2. Use the Arrange-Act-Assert pattern
3. Include both positive and negative test cases
4. Test edge cases and error conditions
5. Use descriptive test names that explain the scenario being tested

## Dependencies

The test project references:
- Core.Domain (for entity testing)
- Infrastructure.Persistence (for database context testing)
- Lambdas.Users (for service testing)
- BuildingBlocks.Observability (for logging configuration testing)
