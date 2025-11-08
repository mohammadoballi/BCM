using BCM;
using BCM.Controllers;
using BCM.Managment.Base.DTOs;
using BCM.Managment.Card.DTOs;
using BCM.Managment.Card.Manager;
using BCM.Models.Entites;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace BCM.Tests.Controllers
{
    public class CardControllerTests
    {
        private readonly Mock<ICardManager> _mockCardManager;
        private readonly CardController _controller;

        public CardControllerTests()
        {
            _mockCardManager = new Mock<ICardManager>();
            _controller = new CardController(_mockCardManager.Object);
        }

        #region GetAllCards Tests

        [Fact]
        public async Task GetAllCards_AsAdministrator_ReturnsOk()
        {
            // Arrange
            SetupAdministratorUser();
            var request = new CardFilterRequest { PageIndex = 1, PageSize = 10 };
            var expectedResponse = DefaultResponse<IEnumerable<CardDetailsResponse>>.SuccessResponse(
                new List<CardDetailsResponse>()
            );
            _mockCardManager.Setup(m => m.GetAll(It.IsAny<CardFilterRequest>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetAllCards(request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().Be(expectedResponse);
        }

        [Fact]
        public async Task GetAllCards_AsNonAdministrator_ReturnsUnauthorized()
        {
            // Arrange
            SetupNonAdministratorUser();
            var request = new CardFilterRequest();

            // Act
            var result = await _controller.GetAllCards(request);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
            var unauthorizedResult = result as UnauthorizedObjectResult;
            var response = unauthorizedResult!.Value as DefaultResponse<bool>;
            response!.Message_en.Should().Contain("do not have permission");
        }

        [Fact]
        public async Task GetAllCards_ManagerReturnsFailure_ReturnsBadRequest()
        {
            // Arrange
            SetupAdministratorUser();
            var request = new CardFilterRequest();
            var failureResponse = DefaultResponse<IEnumerable<CardDetailsResponse>>.FailureResponse(
                "Error occurred", "حدث خطأ"
            );
            _mockCardManager.Setup(m => m.GetAll(It.IsAny<CardFilterRequest>()))
                .ReturnsAsync(failureResponse);

            // Act
            var result = await _controller.GetAllCards(request);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        #endregion

        #region GetCardById Tests

        [Fact]
        public async Task GetCardById_AsAdministrator_ReturnsOk()
        {
            // Arrange
            SetupAdministratorUser();
            var cardId = 1;
            var expectedCard = new CardDetailsResponse
            {
                Id = cardId,
                Name = "Test User",
                Email = "test@example.com"
            };
            var successResponse = DefaultResponse<CardDetailsResponse>.SuccessResponse(expectedCard);
            _mockCardManager.Setup(m => m.GetById(cardId))
                .ReturnsAsync(successResponse);

            // Act
            var result = await _controller.GetCardById(cardId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var response = okResult!.Value as DefaultResponse<CardDetailsResponse>;
            response!.Data.Id.Should().Be(cardId);
        }

        [Fact]
        public async Task GetCardById_AsNonAdministrator_ReturnsUnauthorized()
        {
            // Arrange
            SetupNonAdministratorUser();

            // Act
            var result = await _controller.GetCardById(1);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        #endregion

        #region CreateCard Tests

        [Fact]
        public async Task CreateCard_AsAdministrator_ReturnsOk()
        {
            // Arrange
            SetupAdministratorUser();
            var request = new CardCreateRequest
            {
                Name = "New Card",
                Email = "new@example.com",
                Phone = "1234567890",
                Address = "Test Address",
                Gender = Models.Enums.Gender.Male,
                BirthDate = DateTime.Now.AddYears(-30)
            };
            var successResponse = DefaultResponse<bool>.SuccessResponse(true);
            _mockCardManager.Setup(m => m.Create(It.IsAny<CardCreateRequest>()))
                .ReturnsAsync(successResponse);

            // Act
            var result = await _controller.CreateCard(request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _mockCardManager.Verify(m => m.Create(request), Times.Once);
        }

        [Fact]
        public async Task CreateCard_AsNonAdministrator_ReturnsUnauthorized()
        {
            // Arrange
            SetupNonAdministratorUser();
            var request = new CardCreateRequest();

            // Act
            var result = await _controller.CreateCard(request);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
            _mockCardManager.Verify(m => m.Create(It.IsAny<CardCreateRequest>()), Times.Never);
        }

        #endregion

        #region UpdateCard Tests

        [Fact]
        public async Task UpdateCard_AsAdministrator_ReturnsOk()
        {
            // Arrange
            SetupAdministratorUser();
            var cardId = 1;
            var request = new CardUpdateRequest
            {
                Name = "Updated Name"
            };
            var successResponse = DefaultResponse<bool>.SuccessResponse(true);
            _mockCardManager.Setup(m => m.Update(cardId, It.IsAny<CardUpdateRequest>()))
                .ReturnsAsync(successResponse);

            // Act
            var result = await _controller.CreateCard(cardId, request);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _mockCardManager.Verify(m => m.Update(cardId, request), Times.Once);
        }

        [Fact]
        public async Task UpdateCard_AsNonAdministrator_ReturnsUnauthorized()
        {
            // Arrange
            SetupNonAdministratorUser();
            var request = new CardUpdateRequest();

            // Act
            var result = await _controller.CreateCard(1, request);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        #endregion

        #region DeleteCard Tests

        [Fact]
        public async Task DeleteCard_AsAdministrator_ReturnsOk()
        {
            // Arrange
            SetupAdministratorUser();
            var cardId = 1;
            var successResponse = DefaultResponse<bool>.SuccessResponse(true);
            _mockCardManager.Setup(m => m.Delete(cardId))
                .ReturnsAsync(successResponse);

            // Act
            var result = await _controller.DeleteCard(cardId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _mockCardManager.Verify(m => m.Delete(cardId), Times.Once);
        }

        [Fact]
        public async Task DeleteCard_AsNonAdministrator_ReturnsUnauthorized()
        {
            // Arrange
            SetupNonAdministratorUser();

            // Act
            var result = await _controller.DeleteCard(1);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        #endregion

        #region UploadFile Tests

        [Fact]
        public async Task UploadFile_AsAdministrator_ValidFile_ReturnsOk()
        {
            // Arrange
            SetupAdministratorUser();
            var file = CreateMockFile("test.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            var successResponse = DefaultResponse<bool>.SuccessResponse(true);
            _mockCardManager.Setup(m => m.ImportFile(It.IsAny<IFormFile>()))
                .ReturnsAsync(successResponse);

            // Act
            var result = await _controller.UploadFile(file);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _mockCardManager.Verify(m => m.ImportFile(file), Times.Once);
        }

        [Fact]
        public async Task UploadFile_NullFile_ReturnsBadRequest()
        {
            // Arrange
            SetupAdministratorUser();

            // Act
            var result = await _controller.UploadFile(null!);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            var response = badRequestResult!.Value as DefaultResponse<bool>;
            response!.Message_en.Should().Contain("No file provided");
        }

        [Fact]
        public async Task UploadFile_EmptyFile_ReturnsBadRequest()
        {
            // Arrange
            SetupAdministratorUser();
            var emptyFile = CreateMockFile("empty.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 0);

            // Act
            var result = await _controller.UploadFile(emptyFile);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            var response = badRequestResult!.Value as DefaultResponse<bool>;
            response!.Message_en.Should().Contain("file is empty");
        }

        [Fact]
        public async Task UploadFile_UnsupportedFileType_ReturnsBadRequest()
        {
            // Arrange
            SetupAdministratorUser();
            var txtFile = CreateMockFile("test.txt", "text/plain");

            // Act
            var result = await _controller.UploadFile(txtFile);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            var response = badRequestResult!.Value as DefaultResponse<bool>;
            response!.Message_en.Should().Contain("Unsupported file type");
        }

        [Fact]
        public async Task UploadFile_AsNonAdministrator_ReturnsUnauthorized()
        {
            // Arrange
            SetupNonAdministratorUser();
            var file = CreateMockFile("test.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

            // Act
            var result = await _controller.UploadFile(file);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        #endregion

        #region GenerateQrCode Tests

        [Fact]
        public async Task GenerateQrCode_AsAdministrator_ReturnsOk()
        {
            // Arrange
            SetupAdministratorUser();
            var cardId = 1;
            var qrCode = "base64encodedqrcode";
            var successResponse = DefaultResponse<string>.SuccessResponse(qrCode);
            _mockCardManager.Setup(m => m.GenerateQrCodeForCard(cardId))
                .ReturnsAsync(successResponse);

            // Act
            var result = await _controller.GenerateQrCode(cardId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            var response = okResult!.Value as DefaultResponse<string>;
            response!.Data.Should().Be(qrCode);
        }

        [Fact]
        public async Task GenerateQrCode_AsNonAdministrator_ReturnsUnauthorized()
        {
            // Arrange
            SetupNonAdministratorUser();

            // Act
            var result = await _controller.GenerateQrCode(1);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        #endregion

        #region ImportQrCode Tests

        [Fact]
        public async Task ImportQrCode_AsAdministrator_ReturnsOk()
        {
            // Arrange
            SetupAdministratorUser();
            var qrFile = CreateMockFile("qr.png", "image/png");
            var cardResponse = new CardDetailsResponse { Id = 1, Name = "QR Card" };
            var successResponse = DefaultResponse<CardDetailsResponse>.SuccessResponse(cardResponse);
            _mockCardManager.Setup(m => m.CreateCardFromQrCode(It.IsAny<IFormFile>()))
                .ReturnsAsync(successResponse);

            // Act
            var result = await _controller.ImportQrCode(qrFile);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _mockCardManager.Verify(m => m.CreateCardFromQrCode(qrFile), Times.Once);
        }

        [Fact]
        public async Task ImportQrCode_AsNonAdministrator_ReturnsUnauthorized()
        {
            // Arrange
            SetupNonAdministratorUser();
            var qrFile = CreateMockFile("qr.png", "image/png");

            // Act
            var result = await _controller.ImportQrCode(qrFile);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        #endregion

        #region Helper Methods

        private void SetupAdministratorUser()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, "Administrator"),
                new Claim(ClaimTypes.Name, "admin@example.com")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        private void SetupNonAdministratorUser()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, "User"),
                new Claim(ClaimTypes.Name, "user@example.com")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        private IFormFile CreateMockFile(string fileName, string contentType, long length = 100)
        {
            var content = new byte[length];
            var stream = new MemoryStream(content);

            return new FormFile(stream, 0, length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };
        }

        #endregion
    }
}
