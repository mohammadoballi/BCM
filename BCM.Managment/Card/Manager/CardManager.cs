using BCM.Managment.Base;
using BCM.Managment.Base.DTOs;
using BCM.Managment.Card.DTOs;
using BCM.Models.Data;
using BCM.Models.Entites;
using BCM.Models.Enums;
using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml.Linq;
using ZXing;
using ZXing.Common;
using ZXing.CoreCompat.System.Drawing;
using ZXing.QrCode;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BCM.Managment.Card.Manager
{
    public class CardManager : BaseManager, ICardManager
    {
        public CardManager(IConfiguration configuration, AppDbContext context) : base(configuration, context)
        {

        }

        public async Task<DefaultResponse<bool>> Create(CardCreateRequest card)
        {
            try
            {
                if (card == null)
                    return DefaultResponse<bool>.FailureResponse("Invalid card data", "بيانات البطاقة غير صالحة");

                var newCard = new BusinessCard
                {
                    Name = card.Name,
                    Address = card.Address,
                    Gender = (Gender)card.Gender,
                    BirthDate = card.BirthDate,
                    Email = card.Email,
                    Phone = card.Phone
                };


                if (card.Image != null)
                {
                    using var ms = new MemoryStream();
                    card.Image.CopyTo(ms);
                    var fileBytes = ms.ToArray();
                    newCard.ImageBase64 = Convert.ToBase64String(fileBytes);
                }

                await _context.BusinessCard.AddAsync(newCard);
                await _context.SaveChangesAsync();
                return DefaultResponse<bool>.SuccessResponse(true, "Card created successfully", "تم إنشاء البطاقة بنجاح");
            }
            catch (Exception ex)
            {
                return DefaultResponse<bool>.FailureResponse("An error occurred while Creating the card: " + ex.Message, "حدث خطأ أثناء انشاء البطاقة: " + ex.Message);

            }

        }

        public async Task<DefaultResponse<bool>> Delete(int id)
        {
            try
            {
                var card = await _context.BusinessCard.FirstOrDefaultAsync(x=>x.Id == id);
                if (card == null)
                    return DefaultResponse<bool>.FailureResponse("Card not found", "البطاقة غير موجودة");
                _context.BusinessCard.Remove(card);
                await _context.SaveChangesAsync();
                return DefaultResponse<bool>.SuccessResponse(true, "Card deleted successfully", "تم حذف البطاقة بنجاح");

            }
            catch (Exception ex)
            {
                return DefaultResponse<bool>.FailureResponse("An error occurred while deleting the card: " + ex.Message, "حدث خطأ أثناء حذف البطاقة: " + ex.Message);
            }

        }

        public async Task<DefaultResponse<IEnumerable<CardDetailsResponse>>> GetAll(CardFilterRequest request)
        {
            try
            {
                IQueryable<BusinessCard> query = _context.BusinessCard.AsQueryable();

                // Filters
                if (request.Id != null)
                    query = query.Where(c => c.Id == request.Id);

                if (!string.IsNullOrEmpty(request.Name))
                    query = query.Where(c => c.Name.Contains(request.Name));

                if (request.Gender != null)
                    query = query.Where(c => c.Gender == request.Gender);

                if (!string.IsNullOrEmpty(request.Email))
                    query = query.Where(c => c.Email.Contains(request.Email));

                if (!string.IsNullOrEmpty(request.Phone))
                    query = query.Where(c => c.Phone.Contains(request.Phone));

                // Total count BEFORE pagination
                int totalCount = await query.CountAsync();

                // Pagination
                int pageIndex = request.PageIndex.Value;
                int pageSize = request.PageSize.Value; 
                query = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);

                var result = await query.ToListAsync();

                // Build pagination info
                var pagination = new Pagination
                {
                    Index = pageIndex,
                    Size = pageSize,
                    Total = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };

                if (result.Any())
                {
                    var mappedResult = result.Select(c => new CardDetailsResponse
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Gender = c.Gender.ToDisplayString(),
                        Phone = c.Phone,
                        Address = c.Address,
                        BirthDate = c.BirthDate,
                        Email = c.Email,
                        Image = c.ImageBase64
                    });

                    return DefaultResponse<IEnumerable<CardDetailsResponse>>.SuccessResponse(mappedResult, pagination: pagination);
                }
                else
                {
                    return DefaultResponse<IEnumerable<CardDetailsResponse>>.SuccessResponse(
                        null,
                        pagination: pagination,
                        message_ar: "لم يتم العثور على بطاقات تطابق معايير البحث.",
                        message_en: "No cards found matching the filter criteria."
                    );
                }
            }
            catch (Exception ex)
            {
                return DefaultResponse<IEnumerable<CardDetailsResponse>>.FailureResponse(
                    "An error occurred while retrieving cards: " + ex.Message,
                    "حدث خطأ أثناء استرجاع البطاقات: " + ex.Message
                );
            }
        }



        public async Task<DefaultResponse<CardDetailsResponse>> GetById(int id)
        {
            try
            {
                var card = await _context.BusinessCard.FirstOrDefaultAsync(x=>x.Id==id);
                if (card == null)
                    return DefaultResponse<CardDetailsResponse>.FailureResponse("Card not found", "البطاقة غير موجودة");

                var mappedCard = new CardDetailsResponse
                {
                    Phone = card.Phone,
                    Name = card.Name,
                    Address = card.Address,
                    BirthDate = card.BirthDate,
                    Email = card.Email,
                    Id = card.Id,
                    Image = card.ImageBase64,
                    Gender = card.Gender.ToDisplayString()
                };
                return DefaultResponse<CardDetailsResponse>.SuccessResponse(mappedCard);
            }
            catch (Exception ex)
            {
                return DefaultResponse<CardDetailsResponse>.FailureResponse("An error occurred while retrieving the card: " + ex.Message, "حدث خطأ أثناء استرجاع البطاقة: " + ex.Message);
            }
        }


        public async Task<DefaultResponse<bool>> Update(int id, CardUpdateRequest card)
        {
            try
            {
                var cardInDb = await _context.BusinessCard.FindAsync(id);
                if (cardInDb == null)
                    return DefaultResponse<bool>.FailureResponse("Card not found", "البطاقة غير موجودة");

                if (!string.IsNullOrEmpty(card.Name))
                    cardInDb.Name = card.Name;
                if (!string.IsNullOrEmpty(card.Address))
                    cardInDb.Address = card.Address;
                if (!string.IsNullOrEmpty(card.Email))
                    cardInDb.Email = card.Email;
                if (!string.IsNullOrEmpty(card.Phone))
                    cardInDb.Phone = card.Phone;
                if (card.Gender.HasValue)
                    cardInDb.Gender = (Gender)card.Gender;
                if (card.BirthDate.HasValue)
                    cardInDb.BirthDate = card.BirthDate.Value;

                if (card.Image != null)
                {
                    using var ms = new MemoryStream();
                    await card.Image.CopyToAsync(ms);
                    var fileBytes = ms.ToArray();
                    cardInDb.ImageBase64 = Convert.ToBase64String(fileBytes);
                }
                cardInDb.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return DefaultResponse<bool>.SuccessResponse(true, "Card updated successfully", "تم تحديث البطاقة بنجاح");
            }
            catch (Exception ex)
            {
                return DefaultResponse<bool>.FailureResponse("An error occurred while updating the card: " + ex.Message, "حدث خطأ أثناء تحديث البطاقة: " + ex.Message);

            }
        }



        public async Task<DefaultResponse<bool>> ImportFile(IFormFile file)
        {
            try
            {
                var cards = new List<BusinessCard>();
                int skipped = 0;

                if (file == null || file.Length == 0)
                    return DefaultResponse<bool>.FailureResponse("File is empty", "الملف فارغ");

                var extension = Path.GetExtension(file.FileName).ToLower();

                if (extension == ".xlsx")
                {
                    using var mem = new MemoryStream();
                    await file.CopyToAsync(mem);
                    mem.Position = 0;

                    byte[] header = new byte[2];
                    mem.Read(header, 0, 2);
                    mem.Position = 0;

                    bool isExcel = header[0] == 0x50 && header[1] == 0x4B; 

                    if (!isExcel)
                    {
                        return DefaultResponse<bool>.FailureResponse(
                            "The uploaded file is not a valid Excel .xlsx file.",
                            "الملف المرفوع ليس ملف Excel صالح (.xlsx)."
                        );
                    }

                    try
                    {
                        using var workbook = new XLWorkbook(mem);
                        var worksheet = workbook.Worksheets.FirstOrDefault();

                        if (worksheet == null)
                            return DefaultResponse<bool>.FailureResponse("No worksheets found", "لم يتم العثور على أوراق عمل في الملف");

                        var rows = worksheet.RangeUsed()?.RowsUsed()?.Skip(1);
                        if (rows == null || !rows.Any())
                            return DefaultResponse<bool>.FailureResponse("No data found", "لم يتم العثور على بيانات في الملف");

                        foreach (var row in rows)
                        {
                            var name = row.Cell(1).GetValue<string>()?.Trim();
                            var genderStr = row.Cell(2).GetValue<string>()?.Trim();
                            var email = row.Cell(3).GetValue<string>()?.Trim();
                            var phone = row.Cell(4).GetValue<string>()?.Trim();
                            var birthDateStr = row.Cell(5).GetValue<string>()?.Trim();
                            var address = row.Cell(6).GetValue<string>()?.Trim();
                            var imageBase64 = row.Cell(7).GetValue<string>()?.Trim();

                            // Validation
                            if (string.IsNullOrWhiteSpace(name) ||
                                string.IsNullOrWhiteSpace(email) ||
                                string.IsNullOrWhiteSpace(phone) ||
                                string.IsNullOrWhiteSpace(genderStr) ||
                                string.IsNullOrWhiteSpace(address) ||
                                string.IsNullOrWhiteSpace(birthDateStr) ||
                                !DateTime.TryParse(birthDateStr, out DateTime birthDate))
                            {
                                skipped++;
                                continue;
                            }

                            Enum.TryParse(genderStr, true, out Gender gender);

                            cards.Add(new BusinessCard
                            {
                                Name = name,
                                Email = email,
                                Phone = phone,
                                Gender = gender,
                                CreatedAt = DateTime.UtcNow,
                                Address = address,
                                ImageBase64 = imageBase64 ?? string.Empty,
                                BirthDate = birthDate
                            });
                        }
                    }
             
                    catch (Exception ex)
                    {
                        return DefaultResponse<bool>.FailureResponse(
                            "Corrupted Excel file: " + ex.Message,
                            "الملف تالف أو غير صالح كملف Excel: " + ex.Message
                        );
                    }
                }

                else if (extension == ".csv")
                {
                    using var stream = file.OpenReadStream();
                    using var reader = new StreamReader(stream);
                    var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        PrepareHeaderForMatch = args => args.Header.ToLower(),
                        MissingFieldFound = null,
                        HeaderValidated = null, 
                        TrimOptions = TrimOptions.Trim,
                    };

                    using var csv = new CsvReader(reader, config);
                    var records = csv.GetRecords<ImportXlsxFileDTO>().ToList();

                    foreach (var record in records)
                    {
                        string name = record.name?.Trim();
                        string email = record.email?.Trim();
                        string phone = record.phone?.Trim();
                        string genderStr = record.gender?.Trim();
                        string address = record.address?.Trim();
                        string birthDateStr = record.birthDate?.Trim();
                        string imageBase64 = record?.image?.Trim();

                        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) ||
                            string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(genderStr) ||
                            string.IsNullOrEmpty(address) || string.IsNullOrEmpty(birthDateStr) ||
                            !DateTime.TryParse(birthDateStr, out DateTime birthDate))
                        {
                            skipped++;
                            continue;
                        }

                        Enum.TryParse(genderStr, out Gender gender);

                        cards.Add(new BusinessCard
                        {
                            Name = name,
                            Email = email,
                            Phone = phone,
                            Gender = gender,
                            CreatedAt = DateTime.UtcNow,
                            Address = address,
                            ImageBase64 = imageBase64 ?? "",
                            BirthDate = birthDate
                        });
                    }
                }
                else if (extension == ".xml")
                {
                    try
                    {
                        using var stream = file.OpenReadStream();
                        using var reader = new StreamReader(stream, System.Text.Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
                        var xmlContent = await reader.ReadToEndAsync();
                        
                        if (string.IsNullOrWhiteSpace(xmlContent))
                        {
                            return DefaultResponse<bool>.FailureResponse(
                                "XML file is empty",
                                "ملف XML فارغ"
                            );
                        }

                        var doc = XDocument.Parse(xmlContent);
                        
                        if (doc.Root == null)
                        {
                            return DefaultResponse<bool>.FailureResponse(
                                "XML file has no root element",
                                "ملف XML لا يحتوي على عنصر جذر"
                            );
                        }

                        foreach (var x in doc.Root.Elements("Card"))
                        {
                            var name = x.Element("Name")?.Value?.Trim();
                            var genderStr = x.Element("Gender")?.Value?.Trim();
                            var email = x.Element("Email")?.Value?.Trim();
                            var phone = x.Element("Phone")?.Value?.Trim();
                            var birthDateStr = x.Element("BirthDate")?.Value?.Trim();
                            var address = x.Element("Address")?.Value?.Trim();
                            var imageBase64 = x.Element("ImageBase64")?.Value?.Trim();

                            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) ||
                                string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(genderStr) ||
                                string.IsNullOrEmpty(address) || string.IsNullOrEmpty(birthDateStr) ||
                                !DateTime.TryParse(birthDateStr, out DateTime birthDate))
                            {
                                skipped++;
                                continue;
                            }

                            Enum.TryParse(genderStr, out Gender gender);

                            cards.Add(new BusinessCard
                            {
                                Name = name,
                                Email = email,
                                Phone = phone,
                                Gender = gender,
                                CreatedAt = DateTime.UtcNow,
                                Address = address,
                                ImageBase64 = imageBase64 ?? "",
                                BirthDate = birthDate
                            });
                        }
                    }
                    catch (System.Xml.XmlException xmlEx)
                    {
                        return DefaultResponse<bool>.FailureResponse(
                            $"Invalid XML format: {xmlEx.Message}",
                            $"تنسيق XML غير صالح: {xmlEx.Message}"
                        );
                    }
                }
                else
                {
                    return DefaultResponse<bool>.FailureResponse(
                        "Unsupported file type",
                        "نوع الملف غير مدعوم"
                    );
                }

                if (!cards.Any())
                    return DefaultResponse<bool>.FailureResponse(
                        "No valid cards found in the file",
                        "لم يتم العثور على بطاقات صالحة في الملف"
                    );

                await _context.BusinessCard.AddRangeAsync(cards);
                await _context.SaveChangesAsync();

                int added = cards.Count;

                return DefaultResponse<bool>.SuccessResponse(
                    true,
                    $"Cards imported successfully. Added: {added}, Skipped: {skipped}",
                    $"تم استيراد البطاقات بنجاح. تم الإضافة: {added}، تم التخطي: {skipped}"
                );
            }
            catch (Exception ex)
            {
                return DefaultResponse<bool>.FailureResponse(
                    "An error occurred while importing cards: " + ex.Message,
                    "حدث خطأ أثناء استيراد البطاقات: " + ex.Message
                );
            }
        }

        public async Task<DefaultResponse<string>> GenerateQrCodeForCard(int id)
        {
            try
            {
                var card = await _context.BusinessCard.FindAsync(id);
                if (card == null)
                    return DefaultResponse<string>.FailureResponse("Card not found", "البطاقة غير موجودة");

                string qrContent = $"Name: {card.Name}\nEmail: {card.Email}\nPhone: {card.Phone}\nAddress: {card.Address}";

                var writer = new BarcodeWriterPixelData
                {
                    Format = BarcodeFormat.QR_CODE,
                    Options = new QrCodeEncodingOptions
                    {
                        Height = 250,
                        Width = 250,
                        Margin = 1
                    }
                };

                var pixelData = writer.Write(qrContent);

                using var bitmap = new Bitmap(pixelData.Width, pixelData.Height, PixelFormat.Format32bppRgb);
                var bitmapData = bitmap.LockBits(new Rectangle(0, 0, pixelData.Width, pixelData.Height),
                                                 ImageLockMode.WriteOnly, bitmap.PixelFormat);
                try
                {
                    System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0, pixelData.Pixels.Length);
                }
                finally
                {
                    bitmap.UnlockBits(bitmapData);
                }

                using var ms = new MemoryStream();
                bitmap.Save(ms, ImageFormat.Png);

                string base64Qr = Convert.ToBase64String(ms.ToArray());

                return DefaultResponse<string>.SuccessResponse(base64Qr, "QR code generated successfully", "تم إنشاء رمز الاستجابة السريعة بنجاح");
            }
            catch (Exception ex)
            {
                return DefaultResponse<string>.FailureResponse(
                    "An error occurred while generating QR code: " + ex.Message,
                    "حدث خطأ أثناء إنشاء رمز الاستجابة السريعة: " + ex.Message
                );
            }
        }



        public async Task<DefaultResponse<CardDetailsResponse>> CreateCardFromQrCode(IFormFile qrImageFile)
        {
            try
            {
                if (qrImageFile == null || qrImageFile.Length == 0)
                    return DefaultResponse<CardDetailsResponse>.FailureResponse("QR code image is missing", "صورة رمز QR مفقودة");

                using var ms = new MemoryStream();
                await qrImageFile.CopyToAsync(ms);
                using var bitmap = new Bitmap(ms);

                var reader = new BarcodeReaderGeneric
                {
                    AutoRotate = true,
                    TryInverted = true,
                    Options = new DecodingOptions
                    {
                        TryHarder = true,
                        PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.QR_CODE }
                    }
                };

                var result = reader.Decode(bitmap);

                if (result == null)
                    return DefaultResponse<CardDetailsResponse>.FailureResponse("Unable to decode the QR code", "تعذر قراءة رمز QR");

                var qrText = result.Text;
                var data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);



                foreach (var line in qrText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    Console.WriteLine($"Processing line: '{line}'");
                    var colonIndex = line.IndexOf(':');
                    if (colonIndex > 0)
                    {
                        var key = line.Substring(0, colonIndex).Trim();
                        var value = line.Substring(colonIndex + 1).Trim();
                        data[key] = value;
                        Console.WriteLine($"Added: {key} = {value}");
                    }
                }

                Console.WriteLine($"Total keys parsed: {data.Count}");

                if (!data.ContainsKey("Name") || !data.ContainsKey("Email") || !data.ContainsKey("Phone"))
                    return DefaultResponse<CardDetailsResponse>.FailureResponse(
                        $"QR code missing required fields. Found: {string.Join(", ", data.Keys)}. Raw text: {qrText}",
                        $"رمز QR يفتقد الحقول المطلوبة. تم العثور على: {string.Join(", ", data.Keys)}"
                    );

                var card = new BusinessCard
                {
                    Name = data["Name"],
                    Email = data["Email"],
                    Phone = data["Phone"],
                    Address = data.ContainsKey("Address") ? data["Address"] : null,
                    CreatedAt = DateTime.UtcNow
                };

                if (data.ContainsKey("BirthDate") && DateTime.TryParse(data["BirthDate"], out var birthDate))
                    card.BirthDate = birthDate;

                if (data.ContainsKey("Gender") && Enum.TryParse<Gender>(data["Gender"], true, out var gender))
                    card.Gender = gender;

                await _context.BusinessCard.AddAsync(card);
                await _context.SaveChangesAsync();

                var response = new CardDetailsResponse
                {
                    Id = card.Id,
                    Name = card.Name,
                    Email = card.Email,
                    Phone = card.Phone,
                    Address = card.Address,
                    BirthDate = card.BirthDate,
                    Gender = card.Gender.ToDisplayString(),
                    Image = card.ImageBase64
                };

                return DefaultResponse<CardDetailsResponse>.SuccessResponse(response,
                    "Card created from QR code successfully",
                    "تم إنشاء البطاقة من رمز QR بنجاح");
            }
            catch (Exception ex)
            {
                return DefaultResponse<CardDetailsResponse>.FailureResponse(
                    $"An error occurred while reading QR code: {ex.Message}",
                    $"حدث خطأ أثناء قراءة رمز QR: {ex.Message}"
                );
            }
        }

    }

}
