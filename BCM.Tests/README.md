# BCM.Tests - Unit Test Suite

This project contains comprehensive unit tests for the BCM (Business Card Management) backend implementation.

## Test Coverage

### 1. **CardManagerTests** (`Managers/CardManagerTests.cs`)
Tests for core business card management functionality:
- **Create Operations**: Card creation with/without images, validation
- **Read Operations**: GetAll with filtering, pagination, GetById
- **Update Operations**: Partial and full updates
- **Delete Operations**: Card deletion and error handling

### 2. **CardManagerImportExportTests** (`Managers/CardManagerImportExportTests.cs`)
**Critical path tests** for data import/export:
- **XLSX Import**: Valid files, invalid rows, empty files, corrupted files, large files
- **CSV Import**: Valid files, missing fields, empty files
- **XML Import**: Valid files, invalid data, malformed XML, empty files
- **QR Code Operations**: Generation and scanning
- **Edge Cases**: Null files, unsupported formats, large datasets

### 3. **CardControllerTests** (`Controllers/CardControllerTests.cs`)
Tests for API controller endpoints:
- **Authorization**: Administrator vs non-administrator access
- **CRUD Operations**: All HTTP methods (GET, POST, PUT, DELETE)
- **File Upload**: Validation, file type checking
- **QR Code Endpoints**: Generation and import

## Technologies Used

- **xUnit**: Test framework
- **Moq**: Mocking framework for dependencies
- **FluentAssertions**: Fluent assertion library for readable tests
- **Entity Framework Core InMemory**: In-memory database for testing
- **ClosedXML**: Excel file generation for test data

## Running Tests

### Visual Studio
1. Open Test Explorer (Test > Test Explorer)
2. Click "Run All" to execute all tests

### Command Line
```bash
dotnet test
```

### With Coverage
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## Test Structure

Each test follows the **Arrange-Act-Assert (AAA)** pattern:
- **Arrange**: Set up test data and dependencies
- **Act**: Execute the method under test
- **Assert**: Verify the expected outcome

## Key Test Scenarios

### Import/Export Critical Paths ✅
- ✅ Valid file imports (XLSX, CSV, XML)
- ✅ Invalid data handling and skipping
- ✅ File format validation
- ✅ Large file processing (100+ records)
- ✅ Corrupted file detection
- ✅ Empty file handling
- ✅ Unsupported file type rejection

### Business Logic ✅
- ✅ Card CRUD operations
- ✅ Filtering and pagination
- ✅ Image handling (Base64 encoding)
- ✅ Data validation
- ✅ Error handling and messages

### Security ✅
- ✅ Role-based authorization
- ✅ Administrator-only operations
- ✅ Unauthorized access prevention

## Adding New Tests

When adding new tests:
1. Follow the existing naming convention: `MethodName_Scenario_ExpectedResult`
2. Use FluentAssertions for readable assertions
3. Mock external dependencies using Moq
4. Use in-memory database for data access tests
5. Clean up resources in `Dispose()` method

## Test Data

Test data is generated programmatically in helper methods:
- `SeedTestData()`: Creates sample business cards
- `CreateValidXlsxFile()`: Generates test Excel files
- `CreateValidCsvFile()`: Generates test CSV files
- `CreateValidXmlFile()`: Generates test XML files

## Continuous Integration

These tests are designed to run in CI/CD pipelines:
- Fast execution (in-memory database)
- No external dependencies
- Deterministic results
- Comprehensive coverage of critical paths
