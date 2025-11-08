using BCM.Managment.Card.Manager;
using BCM.Models.Data;
using BCM.Models.Entites;
using BCM.Models.Enums;
using ClosedXML.Excel;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Xml.Linq;
using Xunit;

namespace BCM.Tests.Managers
{
    /// <summary>
    /// Critical tests for data import/export functionality
    /// </summary>
    public class CardManagerImportExportTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly CardManager _cardManager;

        public CardManagerImportExportTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);

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

        #region XLSX Import Tests

        [Fact]
        public async Task ImportFile_ValidXlsxFile_ImportsSuccessfully()
        {
            // Arrange
            var xlsxFile = CreateValidXlsxFile();

            // Act
            var result = await _cardManager.ImportFile(xlsxFile);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Message_en.Should().Contain("Cards imported successfully");
            
            var cards = await _context.BusinessCard.ToListAsync();
            cards.Should().HaveCount(2);
            cards.Should().Contain(c => c.Name == "John Doe" && c.Email == "john@example.com");
            cards.Should().Contain(c => c.Name == "Jane Smith" && c.Email == "jane@example.com");
        }

        [Fact]
        public async Task ImportFile_XlsxWithInvalidRows_SkipsInvalidRows()
        {
            // Arrange
            var xlsxFile = CreateXlsxFileWithInvalidRows();

            // Act
            var result = await _cardManager.ImportFile(xlsxFile);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Message_en.Should().Contain("Skipped");
            
            var cards = await _context.BusinessCard.ToListAsync();
            cards.Should().HaveCount(1); // Only 1 valid row
            cards.First().Name.Should().Be("Valid User");
        }

        [Fact]
        public async Task ImportFile_EmptyXlsxFile_ReturnsFailure()
        {
            // Arrange
            var xlsxFile = CreateEmptyXlsxFile();

            // Act
            var result = await _cardManager.ImportFile(xlsxFile);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Message_en.Should().Contain("No data found");
        }

        [Fact]
        public async Task ImportFile_CorruptedXlsxFile_ReturnsFailure()
        {
            // Arrange
            var corruptedFile = CreateCorruptedXlsxFile();

            // Act
            var result = await _cardManager.ImportFile(corruptedFile);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Message_en.Should().Contain("not a valid Excel");
        }

        #endregion

        #region CSV Import Tests

        [Fact]
        public async Task ImportFile_ValidCsvFile_ImportsSuccessfully()
        {
            // Arrange
            var csvFile = CreateValidCsvFile();

            // Act
            var result = await _cardManager.ImportFile(csvFile);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Message_en.Should().Contain("Cards imported successfully");
            
            var cards = await _context.BusinessCard.ToListAsync();
            cards.Should().HaveCount(2);
            cards.Should().Contain(c => c.Name == "Alice Johnson");
            cards.Should().Contain(c => c.Name == "Bob Williams");
        }

        [Fact]
        public async Task ImportFile_CsvWithMissingFields_SkipsInvalidRows()
        {
            // Arrange
            var csvFile = CreateCsvFileWithMissingFields();

            // Act
            var result = await _cardManager.ImportFile(csvFile);

            // Assert
            result.IsSuccess.Should().BeTrue();
            var cards = await _context.BusinessCard.ToListAsync();
            cards.Should().HaveCount(1); // Only complete rows imported
        }

        [Fact]
        public async Task ImportFile_EmptyCsvFile_ReturnsFailure()
        {
            // Arrange
            var csvContent = "name,gender,email,phone,birthDate,address,image\n";
            var csvFile = CreateCsvFileFromContent(csvContent);

            // Act
            var result = await _cardManager.ImportFile(csvFile);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Message_en.Should().Contain("No valid cards found");
        }

        #endregion

        #region XML Import Tests

        [Fact]
        public async Task ImportFile_ValidXmlFile_ImportsSuccessfully()
        {
            // Arrange
            var xmlFile = CreateValidXmlFile();

            // Act
            var result = await _cardManager.ImportFile(xmlFile);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Message_en.Should().Contain("Cards imported successfully");
            
            var cards = await _context.BusinessCard.ToListAsync();
            cards.Should().HaveCount(2);
            cards.Should().Contain(c => c.Name == "XML User 1");
            cards.Should().Contain(c => c.Name == "XML User 2");
        }

        [Fact]
        public async Task ImportFile_XmlWithInvalidData_SkipsInvalidEntries()
        {
            // Arrange
            var xmlFile = CreateXmlFileWithInvalidData();

            // Act
            var result = await _cardManager.ImportFile(xmlFile);

            // Assert
            result.IsSuccess.Should().BeTrue();
            var cards = await _context.BusinessCard.ToListAsync();
            cards.Should().HaveCount(1); // Only valid entry
        }

        [Fact]
        public async Task ImportFile_MalformedXmlFile_ReturnsFailure()
        {
            // Arrange
            var xmlContent = "<Cards><Card><Name>Test</Name></Card"; // Missing closing tag
            var xmlFile = CreateXmlFileFromContent(xmlContent);

            // Act
            var result = await _cardManager.ImportFile(xmlFile);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Message_en.Should().Contain("Invalid XML format");
        }

        [Fact]
        public async Task ImportFile_EmptyXmlFile_ReturnsFailure()
        {
            // Arrange
            var xmlContent = "";
            var xmlFile = CreateXmlFileFromContent(xmlContent);

            // Act
            var result = await _cardManager.ImportFile(xmlFile);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Message_en.Should().Contain("File is empty");
        }

        #endregion

        #region General Import Tests

        [Fact]
        public async Task ImportFile_NullFile_ReturnsFailure()
        {
            // Act
            var result = await _cardManager.ImportFile(null!);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Message_en.Should().Contain("File is empty");
        }

        [Fact]
        public async Task ImportFile_UnsupportedFileType_ReturnsFailure()
        {
            // Arrange
            var txtFile = CreateTextFile();

            // Act
            var result = await _cardManager.ImportFile(txtFile);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Message_en.Should().Contain("Unsupported file type");
        }

        [Fact]
        public async Task ImportFile_LargeValidFile_ImportsAllRecords()
        {
            // Arrange
            var largeXlsxFile = CreateLargeXlsxFile(100);

            // Act
            var result = await _cardManager.ImportFile(largeXlsxFile);

            // Assert
            result.IsSuccess.Should().BeTrue();
            var cards = await _context.BusinessCard.ToListAsync();
            cards.Should().HaveCount(100);
        }

        #endregion

        #region QR Code Tests

        [Fact]
        public async Task GenerateQrCodeForCard_ValidCard_ReturnsQrCode()
        {
            // Arrange
            var card = new BusinessCard
            {
                Name = "QR Test User",
                Email = "qr@example.com",
                Phone = "5555555555",
                Address = "QR Address",
                Gender = Gender.Male,
                BirthDate = new DateTime(1990, 1, 1),
                CreatedAt = DateTime.UtcNow
            };
            await _context.BusinessCard.AddAsync(card);
            await _context.SaveChangesAsync();

            // Act
            var result = await _cardManager.GenerateQrCodeForCard(card.Id);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().NotBeNullOrEmpty();
            // QR code should be a base64 string
            result.Data.Should().MatchRegex("^[A-Za-z0-9+/]*={0,2}$");
        }

        [Fact]
        public async Task GenerateQrCodeForCard_NonExistingCard_ReturnsFailure()
        {
            // Act
            var result = await _cardManager.GenerateQrCodeForCard(999);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Message_en.Should().Contain("Card not found");
        }

        #endregion

        #region Helper Methods - XLSX

        private IFormFile CreateValidXlsxFile()
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Cards");

            // Headers
            worksheet.Cell(1, 1).Value = "Name";
            worksheet.Cell(1, 2).Value = "Gender";
            worksheet.Cell(1, 3).Value = "Email";
            worksheet.Cell(1, 4).Value = "Phone";
            worksheet.Cell(1, 5).Value = "BirthDate";
            worksheet.Cell(1, 6).Value = "Address";
            worksheet.Cell(1, 7).Value = "Image";

            // Data rows
            worksheet.Cell(2, 1).Value = "John Doe";
            worksheet.Cell(2, 2).Value = "Male";
            worksheet.Cell(2, 3).Value = "john@example.com";
            worksheet.Cell(2, 4).Value = "1234567890";
            worksheet.Cell(2, 5).Value = "1990-01-01";
            worksheet.Cell(2, 6).Value = "123 Main St";
            worksheet.Cell(2, 7).Value = "";

            worksheet.Cell(3, 1).Value = "Jane Smith";
            worksheet.Cell(3, 2).Value = "Female";
            worksheet.Cell(3, 3).Value = "jane@example.com";
            worksheet.Cell(3, 4).Value = "0987654321";
            worksheet.Cell(3, 5).Value = "1995-05-15";
            worksheet.Cell(3, 6).Value = "456 Oak Ave";
            worksheet.Cell(3, 7).Value = "";

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return new FormFile(stream, 0, stream.Length, "file", "cards.xlsx")
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            };
        }

        private IFormFile CreateXlsxFileWithInvalidRows()
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Cards");

            worksheet.Cell(1, 1).Value = "Name";
            worksheet.Cell(1, 2).Value = "Gender";
            worksheet.Cell(1, 3).Value = "Email";
            worksheet.Cell(1, 4).Value = "Phone";
            worksheet.Cell(1, 5).Value = "BirthDate";
            worksheet.Cell(1, 6).Value = "Address";
            worksheet.Cell(1, 7).Value = "Image";

            // Valid row
            worksheet.Cell(2, 1).Value = "Valid User";
            worksheet.Cell(2, 2).Value = "Male";
            worksheet.Cell(2, 3).Value = "valid@example.com";
            worksheet.Cell(2, 4).Value = "1234567890";
            worksheet.Cell(2, 5).Value = "1990-01-01";
            worksheet.Cell(2, 6).Value = "Valid Address";
            worksheet.Cell(2, 7).Value = "";

            // Invalid row - missing email
            worksheet.Cell(3, 1).Value = "Invalid User";
            worksheet.Cell(3, 2).Value = "Female";
            worksheet.Cell(3, 3).Value = "";
            worksheet.Cell(3, 4).Value = "0987654321";
            worksheet.Cell(3, 5).Value = "1995-05-15";
            worksheet.Cell(3, 6).Value = "Invalid Address";
            worksheet.Cell(3, 7).Value = "";

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return new FormFile(stream, 0, stream.Length, "file", "cards.xlsx")
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            };
        }

        private IFormFile CreateEmptyXlsxFile()
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Cards");

            // Only headers, no data
            worksheet.Cell(1, 1).Value = "Name";
            worksheet.Cell(1, 2).Value = "Gender";
            worksheet.Cell(1, 3).Value = "Email";
            worksheet.Cell(1, 4).Value = "Phone";
            worksheet.Cell(1, 5).Value = "BirthDate";
            worksheet.Cell(1, 6).Value = "Address";
            worksheet.Cell(1, 7).Value = "Image";

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return new FormFile(stream, 0, stream.Length, "file", "empty.xlsx")
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            };
        }

        private IFormFile CreateCorruptedXlsxFile()
        {
            var corruptedData = Encoding.UTF8.GetBytes("This is not a valid XLSX file");
            var stream = new MemoryStream(corruptedData);

            return new FormFile(stream, 0, stream.Length, "file", "corrupted.xlsx")
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            };
        }

        private IFormFile CreateLargeXlsxFile(int recordCount)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Cards");

            // Headers
            worksheet.Cell(1, 1).Value = "Name";
            worksheet.Cell(1, 2).Value = "Gender";
            worksheet.Cell(1, 3).Value = "Email";
            worksheet.Cell(1, 4).Value = "Phone";
            worksheet.Cell(1, 5).Value = "BirthDate";
            worksheet.Cell(1, 6).Value = "Address";
            worksheet.Cell(1, 7).Value = "Image";

            // Generate records
            for (int i = 0; i < recordCount; i++)
            {
                int row = i + 2;
                worksheet.Cell(row, 1).Value = $"User {i}";
                worksheet.Cell(row, 2).Value = i % 2 == 0 ? "Male" : "Female";
                worksheet.Cell(row, 3).Value = $"user{i}@example.com";
                worksheet.Cell(row, 4).Value = $"12345{i:D5}";
                worksheet.Cell(row, 5).Value = "1990-01-01";
                worksheet.Cell(row, 6).Value = $"Address {i}";
                worksheet.Cell(row, 7).Value = "";
            }

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return new FormFile(stream, 0, stream.Length, "file", "large.xlsx")
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            };
        }

        #endregion

        #region Helper Methods - CSV

        private IFormFile CreateValidCsvFile()
        {
            var csvContent = @"name,gender,email,phone,birthDate,address,image
Alice Johnson,Female,alice@example.com,1111111111,1985-03-15,111 First St,
Bob Williams,Male,bob@example.com,2222222222,1990-07-20,222 Second St,";

            return CreateCsvFileFromContent(csvContent);
        }

        private IFormFile CreateCsvFileWithMissingFields()
        {
            var csvContent = @"name,gender,email,phone,birthDate,address,image
Complete User,Male,complete@example.com,1111111111,1985-03-15,Complete Address,
Incomplete User,Female,,2222222222,1990-07-20,Incomplete Address,";

            return CreateCsvFileFromContent(csvContent);
        }

        private IFormFile CreateCsvFileFromContent(string content)
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(bytes);

            return new FormFile(stream, 0, stream.Length, "file", "cards.csv")
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/csv"
            };
        }

        #endregion

        #region Helper Methods - XML

        private IFormFile CreateValidXmlFile()
        {
            var xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Cards>
  <Card>
    <Name>XML User 1</Name>
    <Gender>Male</Gender>
    <Email>xml1@example.com</Email>
    <Phone>3333333333</Phone>
    <BirthDate>1988-11-05</BirthDate>
    <Address>333 Third St</Address>
    <ImageBase64></ImageBase64>
  </Card>
  <Card>
    <Name>XML User 2</Name>
    <Gender>Female</Gender>
    <Email>xml2@example.com</Email>
    <Phone>4444444444</Phone>
    <BirthDate>1992-02-28</BirthDate>
    <Address>444 Fourth St</Address>
    <ImageBase64></ImageBase64>
  </Card>
</Cards>";

            return CreateXmlFileFromContent(xmlContent);
        }

        private IFormFile CreateXmlFileWithInvalidData()
        {
            var xmlContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Cards>
  <Card>
    <Name>Valid XML User</Name>
    <Gender>Male</Gender>
    <Email>valid@example.com</Email>
    <Phone>5555555555</Phone>
    <BirthDate>1990-01-01</BirthDate>
    <Address>Valid Address</Address>
    <ImageBase64></ImageBase64>
  </Card>
  <Card>
    <Name>Invalid XML User</Name>
    <Gender>Female</Gender>
    <Email></Email>
    <Phone>6666666666</Phone>
    <BirthDate>1995-05-15</BirthDate>
    <Address>Invalid Address</Address>
    <ImageBase64></ImageBase64>
  </Card>
</Cards>";

            return CreateXmlFileFromContent(xmlContent);
        }

        private IFormFile CreateXmlFileFromContent(string content)
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(bytes);

            return new FormFile(stream, 0, stream.Length, "file", "cards.xml")
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/xml"
            };
        }

        #endregion

        #region Helper Methods - Other

        private IFormFile CreateTextFile()
        {
            var content = "This is a text file, not a valid import format";
            var bytes = Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(bytes);

            return new FormFile(stream, 0, stream.Length, "file", "cards.txt")
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/plain"
            };
        }

        #endregion
    }
}
