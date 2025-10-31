using BCM.Managment.Base;
using BCM.Managment.Base.DTOs;
using BCM.Managment.Card.DTOs;
using BCM.Models.Data;
using BCM.Models.Entites;
using BCM.Models.Enums;
using ClosedXML.Excel;
using CsvHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml.Linq;
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

        public async Task<DefaultResponse<IEnumerable<CardMinimumResponse>>> GetAll(CardFilterRequest request)
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
                    var mappedResult = result.Select(c => new CardMinimumResponse
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Gender = c.Gender.ToDisplayString(),
                        Phone = c.Phone
                    });

                    return DefaultResponse<IEnumerable<CardMinimumResponse>>.SuccessResponse(mappedResult, pagination: pagination);
                }
                else
                {
                    return DefaultResponse<IEnumerable<CardMinimumResponse>>.SuccessResponse(
                        null,
                        pagination: pagination,
                        message_ar: "لم يتم العثور على بطاقات تطابق معايير البحث.",
                        message_en: "No cards found matching the filter criteria."
                    );
                }
            }
            catch (Exception ex)
            {
                return DefaultResponse<IEnumerable<CardMinimumResponse>>.FailureResponse(
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
                    Image = card.ImageBase64
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
                var extension = Path.GetExtension(file.FileName).ToLower();

                using var stream = file.OpenReadStream();

                if (extension == ".xlsx")
                {
                    using var workbook = new XLWorkbook(stream);
                    var worksheet = workbook.Worksheets.First();
                    var rows = worksheet.RangeUsed().RowsUsed().Skip(1);

                    foreach (var row in rows)
                    {
                        var name = row.Cell(1).GetValue<string>()?.Trim();
                        var email = row.Cell(2).GetValue<string>()?.Trim();
                        var phone = row.Cell(3).GetValue<string>()?.Trim();
                        var genderStr = row.Cell(4).GetValue<string>()?.Trim();
                        var address = row.Cell(5).GetValue<string>()?.Trim();
                        var birthDateStr = row.Cell(6).GetValue<string>()?.Trim();
                        var imageBase64 = row.Cell(7).GetValue<string>()?.Trim();

                        // Skip if required fields missing or invalid
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
                else if (extension == ".csv")
                {
                    using var reader = new StreamReader(stream);
                    using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                    var records = csv.GetRecords<dynamic>();

                    foreach (var record in records)
                    {
                        string name = record.Name?.Trim();
                        string email = record.Email?.Trim();
                        string phone = record.Phone?.Trim();
                        string genderStr = record.Gender?.Trim();
                        string address = record.Address?.Trim();
                        string birthDateStr = record.BirthDate?.Trim();
                        string imageBase64 = record.ImageBase64?.Trim();

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
                    var doc = XDocument.Load(stream);
                    foreach (var x in doc.Root.Elements("Card"))
                    {
                        var name = x.Element("Name")?.Value?.Trim();
                        var email = x.Element("Email")?.Value?.Trim();
                        var phone = x.Element("Phone")?.Value?.Trim();
                        var genderStr = x.Element("Gender")?.Value?.Trim();
                        var address = x.Element("Address")?.Value?.Trim();
                        var birthDateStr = x.Element("BirthDate")?.Value?.Trim();
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

                // Save to database
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



    }

}
