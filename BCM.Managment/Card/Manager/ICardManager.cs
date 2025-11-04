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
        /// <summary>
        /// Retrieves all business cards based on the provided filter criteria.
        /// </summary>
        /// <param name="request">Filter criteria for querying cards.</param>
        /// <returns>A response containing a collection of card details.</returns>
        Task<DefaultResponse<IEnumerable<CardDetailsResponse>>> GetAll(CardFilterRequest request);
        
        /// <summary>
        /// Retrieves a specific business card by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the card.</param>
        /// <returns>A response containing the card details.</returns>
        Task<DefaultResponse<CardDetailsResponse>> GetById(int id);
        
        /// <summary>
        /// Creates a new business card.
        /// </summary>
        /// <param name="card">The card creation request containing card details.</param>
        /// <returns>A response indicating success or failure of the operation.</returns>
        Task<DefaultResponse<bool>> Create(CardCreateRequest card);
        
        /// <summary>
        /// Updates an existing business card.
        /// </summary>
        /// <param name="id">The unique identifier of the card to update.</param>
        /// <param name="card">The card update request containing updated details.</param>
        /// <returns>A response indicating success or failure of the operation.</returns>
        Task<DefaultResponse<bool>> Update(int id, CardUpdateRequest card);
        
        /// <summary>
        /// Deletes a business card by its ID.
        /// </summary>
        /// <param name="id">The unique identifier of the card to delete.</param>
        /// <returns>A response indicating success or failure of the operation.</returns>
        Task<DefaultResponse<bool>> Delete(int id);
        
        /// <summary>
        /// Imports business cards from a file (XLSX, CSV, or XML).
        /// </summary>
        /// <param name="File">The file containing card data to import.</param>
        /// <returns>A response indicating success or failure of the import operation.</returns>
        Task<DefaultResponse<bool>> ImportFile(IFormFile File);
        
        /// <summary>
        /// Generates a QR code for a specific business card.
        /// </summary>
        /// <param name="id">The unique identifier of the card.</param>
        /// <returns>A response containing the generated QR code as a string.</returns>
        Task<DefaultResponse<string>> GenerateQrCodeForCard(int id);
        
        /// <summary>
        /// Creates a business card by scanning and processing a QR code image.
        /// </summary>
        /// <param name="qrImageFile">The QR code image file to process.</param>
        /// <returns>A response containing the created card details.</returns>
        Task<DefaultResponse<CardDetailsResponse>> CreateCardFromQrCode(IFormFile qrImageFile);
    }
}
