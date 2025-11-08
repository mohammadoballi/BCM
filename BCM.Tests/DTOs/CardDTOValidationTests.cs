using BCM.Managment.Card.DTOs;
using BCM.Models.Enums;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace BCM.Tests.DTOs
{
    /// <summary>
    /// Tests for DTO validation attributes
    /// </summary>
    public class CardDTOValidationTests
    {
        #region CardCreateRequest Validation Tests

        [Fact]
        public void CardCreateRequest_AllRequiredFields_IsValid()
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
            var validationResults = ValidateModel(request);

            // Assert
            validationResults.Should().BeEmpty();
        }

        [Fact]
        public void CardCreateRequest_MissingName_IsInvalid()
        {
            // Arrange
            var request = new CardCreateRequest
            {
                Name = null!,
                Email = "john@example.com",
                Phone = "1234567890",
                Address = "123 Main St",
                Gender = Gender.Male,
                BirthDate = new DateTime(1990, 1, 1)
            };

            // Act
            var validationResults = ValidateModel(request);

            // Assert
            validationResults.Should().NotBeEmpty();
            validationResults.Should().Contain(v => v.MemberNames.Contains("Name"));
        }

        [Fact]
        public void CardCreateRequest_MissingEmail_IsInvalid()
        {
            // Arrange
            var request = new CardCreateRequest
            {
                Name = "John Doe",
                Email = null!,
                Phone = "1234567890",
                Address = "123 Main St",
                Gender = Gender.Male,
                BirthDate = new DateTime(1990, 1, 1)
            };

            // Act
            var validationResults = ValidateModel(request);

            // Assert
            validationResults.Should().NotBeEmpty();
            validationResults.Should().Contain(v => v.MemberNames.Contains("Email"));
        }

        [Fact]
        public void CardCreateRequest_MissingPhone_IsInvalid()
        {
            // Arrange
            var request = new CardCreateRequest
            {
                Name = "John Doe",
                Email = "john@example.com",
                Phone = null!,
                Address = "123 Main St",
                Gender = Gender.Male,
                BirthDate = new DateTime(1990, 1, 1)
            };

            // Act
            var validationResults = ValidateModel(request);

            // Assert
            validationResults.Should().NotBeEmpty();
            validationResults.Should().Contain(v => v.MemberNames.Contains("Phone"));
        }

        [Fact]
        public void CardCreateRequest_MissingAddress_IsInvalid()
        {
            // Arrange
            var request = new CardCreateRequest
            {
                Name = "John Doe",
                Email = "john@example.com",
                Phone = "1234567890",
                Address = null!,
                Gender = Gender.Male,
                BirthDate = new DateTime(1990, 1, 1)
            };

            // Act
            var validationResults = ValidateModel(request);

            // Assert
            validationResults.Should().NotBeEmpty();
            validationResults.Should().Contain(v => v.MemberNames.Contains("Address"));
        }

        #endregion

        #region CardFilterRequest Tests

        [Fact]
        public void CardFilterRequest_DefaultValues_AreValid()
        {
            // Arrange
            var request = new CardFilterRequest();

            // Assert
            request.PageSize.Should().Be(10);
            request.PageIndex.Should().Be(1);
        }

        [Fact]
        public void CardFilterRequest_CustomPagination_IsValid()
        {
            // Arrange
            var request = new CardFilterRequest
            {
                PageSize = 20,
                PageIndex = 2
            };

            // Assert
            request.PageSize.Should().Be(20);
            request.PageIndex.Should().Be(2);
        }

        [Fact]
        public void CardFilterRequest_WithFilters_IsValid()
        {
            // Arrange
            var request = new CardFilterRequest
            {
                Name = "John",
                Email = "john@example.com",
                Phone = "123",
                Gender = Gender.Male,
                Id = 1
            };

            // Act
            var validationResults = ValidateModel(request);

            // Assert
            validationResults.Should().BeEmpty();
            request.Name.Should().Be("John");
            request.Email.Should().Be("john@example.com");
            request.Phone.Should().Be("123");
            request.Gender.Should().Be(Gender.Male);
            request.Id.Should().Be(1);
        }

        #endregion

        #region CardUpdateRequest Tests

        [Fact]
        public void CardUpdateRequest_PartialUpdate_IsValid()
        {
            // Arrange
            var request = new CardUpdateRequest
            {
                Name = "Updated Name"
            };

            // Act
            var validationResults = ValidateModel(request);

            // Assert
            validationResults.Should().BeEmpty();
            request.Name.Should().Be("Updated Name");
            request.Email.Should().BeNull();
            request.Phone.Should().BeNull();
        }

        [Fact]
        public void CardUpdateRequest_AllFieldsNull_IsValid()
        {
            // Arrange
            var request = new CardUpdateRequest();

            // Act
            var validationResults = ValidateModel(request);

            // Assert
            validationResults.Should().BeEmpty();
        }

        [Fact]
        public void CardUpdateRequest_MultipleFields_IsValid()
        {
            // Arrange
            var request = new CardUpdateRequest
            {
                Name = "New Name",
                Email = "newemail@example.com",
                Phone = "9876543210"
            };

            // Act
            var validationResults = ValidateModel(request);

            // Assert
            validationResults.Should().BeEmpty();
            request.Name.Should().Be("New Name");
            request.Email.Should().Be("newemail@example.com");
            request.Phone.Should().Be("9876543210");
        }

        #endregion

        #region Helper Methods

        private List<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, validationContext, validationResults, true);
            return validationResults;
        }

        #endregion
    }
}
