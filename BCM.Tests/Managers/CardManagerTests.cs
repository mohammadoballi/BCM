using BCM.Managment.Card.DTOs;
using BCM.Managment.Card.Manager;
using BCM.Models.Data;
using BCM.Models.Entites;
using BCM.Models.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Text;
using Xunit;

namespace BCM.Tests.Managers
{
    public class CardManagerTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly CardManager _cardManager;

        public CardManagerTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);

            // Setup configuration
            var inMemorySettings = new Dictionary<string, string>
            {
                {"ConnectionStrings:DefaultConnection", "InMemoryTestDb"}
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();

            _cardManager = new CardManager(_configuration, _context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Create Tests

        [Fact]
        public async Task Create_ValidCard_ReturnsSuccess()
        {
            // Arrange
            var request = new CardCreateRequest
            {
                Name = "John Doe",
                Email = "john@example.com",
                Phone = "1234567890",
                Address = "123 Main St",
                Gender = Gender.Male,
                BirthDate = new DateTime(1990, 1, 1)
            };

            // Act
            var result = await _cardManager.Create(request);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeTrue();
            var card = await _context.BusinessCard.FirstOrDefaultAsync(c => c.Email == "john@example.com");
            card.Should().NotBeNull();
            card!.Name.Should().Be("John Doe");
        }

        [Fact]
        public async Task Create_NullCard_ReturnsFailure()
        {
            // Act
            var result = await _cardManager.Create(null!);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Message_en.Should().Contain("Invalid card data");
        }

        [Fact]
        public async Task Create_WithImage_SavesImageAsBase64()
        {
            // Arrange
            var imageBytes = Encoding.UTF8.GetBytes("fake-image-data");
            var stream = new MemoryStream(imageBytes);
            var formFile = new FormFile(stream, 0, imageBytes.Length, "image", "test.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };

            var request = new CardCreateRequest
            {
                Name = "Jane Doe",
                Email = "jane@example.com",
                Phone = "0987654321",
                Address = "456 Oak Ave",
                Gender = Gender.Female,
                BirthDate = new DateTime(1995, 5, 15),
                Image = formFile
            };

            // Act
            var result = await _cardManager.Create(request);

            // Assert
            result.IsSuccess.Should().BeTrue();
            var card = await _context.BusinessCard.FirstOrDefaultAsync(c => c.Email == "jane@example.com");
            card.Should().NotBeNull();
            card!.ImageBase64.Should().NotBeNullOrEmpty();
        }

        #endregion

        #region GetAll Tests

        [Fact]
        public async Task GetAll_WithNoFilters_ReturnsAllCards()
        {
            // Arrange
            await SeedTestData();
            var request = new CardFilterRequest
            {
                PageIndex = 1,
                PageSize = 10
            };

            // Act
            var result = await _cardManager.GetAll(request);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Should().HaveCount(3);
            result.Pagination.Should().NotBeNull();
            result.Pagination.Total.Should().Be(3);
        }

        [Fact]
        public async Task GetAll_WithNameFilter_ReturnsFilteredCards()
        {
            // Arrange
            await SeedTestData();
            var request = new CardFilterRequest
            {
                Name = "Alice",
                PageIndex = 1,
                PageSize = 10
            };

            // Act
            var result = await _cardManager.GetAll(request);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
            result.Data.First().Name.Should().Contain("Alice");
        }

        [Fact]
        public async Task GetAll_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            await SeedTestData();
            var request = new CardFilterRequest
            {
                PageIndex = 1,
                PageSize = 2
            };

            // Act
            var result = await _cardManager.GetAll(request);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(2);
            result.Pagination.TotalPages.Should().Be(2);
            result.Pagination.Total.Should().Be(3);
        }

        [Fact]
        public async Task GetAll_WithGenderFilter_ReturnsFilteredCards()
        {
            // Arrange
            await SeedTestData();
            var request = new CardFilterRequest
            {
                Gender = Gender.Female,
                PageIndex = 1,
                PageSize = 10
            };

            // Act
            var result = await _cardManager.GetAll(request);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        #endregion

        #region GetById Tests

        [Fact]
        public async Task GetById_ExistingCard_ReturnsCard()
        {
            // Arrange
            var card = new BusinessCard
            {
                Name = "Test User",
                Email = "test@example.com",
                Phone = "1111111111",
                Address = "Test Address",
                Gender = Gender.Male,
                BirthDate = new DateTime(1990, 1, 1),
                CreatedAt = DateTime.UtcNow
            };
            await _context.BusinessCard.AddAsync(card);
            await _context.SaveChangesAsync();

            // Act
            var result = await _cardManager.GetById(card.Id);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Name.Should().Be("Test User");
            result.Data.Email.Should().Be("test@example.com");
        }

        [Fact]
        public async Task GetById_NonExistingCard_ReturnsFailure()
        {
            // Act
            var result = await _cardManager.GetById(999);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Message_en.Should().Contain("Card not found");
        }

        #endregion

        #region Update Tests

        [Fact]
        public async Task Update_ExistingCard_UpdatesSuccessfully()
        {
            // Arrange
            var card = new BusinessCard
            {
                Name = "Original Name",
                Email = "original@example.com",
                Phone = "1234567890",
                Address = "Original Address",
                Gender = Gender.Male,
                BirthDate = new DateTime(1990, 1, 1),
                CreatedAt = DateTime.UtcNow
            };
            await _context.BusinessCard.AddAsync(card);
            await _context.SaveChangesAsync();

            var updateRequest = new CardUpdateRequest
            {
                Name = "Updated Name",
                Email = "updated@example.com"
            };

            // Act
            var result = await _cardManager.Update(card.Id, updateRequest);

            // Assert
            result.IsSuccess.Should().BeTrue();
            var updatedCard = await _context.BusinessCard.FindAsync(card.Id);
            updatedCard!.Name.Should().Be("Updated Name");
            updatedCard.Email.Should().Be("updated@example.com");
            updatedCard.UpdatedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task Update_NonExistingCard_ReturnsFailure()
        {
            // Arrange
            var updateRequest = new CardUpdateRequest
            {
                Name = "Updated Name"
            };

            // Act
            var result = await _cardManager.Update(999, updateRequest);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Message_en.Should().Contain("Card not found");
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_ExistingCard_DeletesSuccessfully()
        {
            // Arrange
            var card = new BusinessCard
            {
                Name = "To Delete",
                Email = "delete@example.com",
                Phone = "1234567890",
                Address = "Delete Address",
                Gender = Gender.Male,
                BirthDate = new DateTime(1990, 1, 1),
                CreatedAt = DateTime.UtcNow
            };
            await _context.BusinessCard.AddAsync(card);
            await _context.SaveChangesAsync();

            // Act
            var result = await _cardManager.Delete(card.Id);

            // Assert
            result.IsSuccess.Should().BeTrue();
            var deletedCard = await _context.BusinessCard.FindAsync(card.Id);
            deletedCard.Should().BeNull();
        }

        [Fact]
        public async Task Delete_NonExistingCard_ReturnsFailure()
        {
            // Act
            var result = await _cardManager.Delete(999);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Message_en.Should().Contain("Card not found");
        }

        #endregion

        #region Helper Methods

        private async Task SeedTestData()
        {
            var cards = new List<BusinessCard>
            {
                new BusinessCard
                {
                    Name = "Alice Smith",
                    Email = "alice@example.com",
                    Phone = "1111111111",
                    Address = "111 First St",
                    Gender = Gender.Female,
                    BirthDate = new DateTime(1985, 3, 15),
                    CreatedAt = DateTime.UtcNow
                },
                new BusinessCard
                {
                    Name = "Bob Johnson",
                    Email = "bob@example.com",
                    Phone = "2222222222",
                    Address = "222 Second St",
                    Gender = Gender.Male,
                    BirthDate = new DateTime(1990, 7, 20),
                    CreatedAt = DateTime.UtcNow
                },
                new BusinessCard
                {
                    Name = "Charlie Brown",
                    Email = "charlie@example.com",
                    Phone = "3333333333",
                    Address = "333 Third St",
                    Gender = Gender.Male,
                    BirthDate = new DateTime(1988, 11, 5),
                    CreatedAt = DateTime.UtcNow
                }
            };

            await _context.BusinessCard.AddRangeAsync(cards);
            await _context.SaveChangesAsync();
        }

        #endregion
    }
}
