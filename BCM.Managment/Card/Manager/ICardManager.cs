using BCM.Managment.Base.DTOs;
using BCM.Managment.Card.DTOs;
using BCM.Models.Entites;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCM.Managment.Card.Manager
{
    public interface ICardManager
    {
        Task<DefaultResponse<IEnumerable<CardDetailsResponse>>> GetAll(CardFilterRequest request);
        Task<DefaultResponse<CardDetailsResponse>> GetById(int id);
        Task<DefaultResponse<bool>> Create(CardCreateRequest card);
        Task<DefaultResponse<bool>> Update(int id, CardUpdateRequest card);
        Task<DefaultResponse<bool>> Delete(int id);
        Task<DefaultResponse<bool>> ImportFile(IFormFile File);
        Task<DefaultResponse<string>> GenerateQrCodeForCard(int id);
        Task<DefaultResponse<CardDetailsResponse>> CreateCardFromQrCode(IFormFile qrImageFile);
    }
}
