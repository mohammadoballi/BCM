using BCM.Managment.Base.DTOs;
using BCM.Managment.Card.DTOs;
using BCM.Managment.Card.Manager;
using BCM.Models.Entites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BCM.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CardController(ICardManager _cardManager) : ControllerBase
    {


        [HttpGet]
        [Route("GetAllCards")]
        [ProducesDefaultResponseType(typeof(DefaultResponse<List<BusinessCard>>))]
        public async Task<IActionResult> GetAllCards([FromQuery]CardFilterRequest request)
        {
            var climes = HttpContext.User.Claims;
            var role = climes.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (role != "Administrator")
                return Unauthorized(DefaultResponse<bool>.FailureResponse(
                 message_en: "You do not have permission to perform this action.",
                 message_ar: "ليس لديك صلاحية لتنفيذ هذا الإجراء."
             ));

            var result = await _cardManager.GetAll(request);
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);

        }

        [HttpGet]
        [Route("GetCardById/{CardId}")]
        [ProducesDefaultResponseType(typeof(DefaultResponse<List<BusinessCard>>))]
        public async Task<IActionResult> GetCardById(int CardId)
        {
            var climes = HttpContext.User.Claims;
            var role = climes.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (role != "Administrator")
                return Unauthorized(DefaultResponse<bool>.FailureResponse(
                 message_en: "You do not have permission to perform this action.",
                 message_ar: "ليس لديك صلاحية لتنفيذ هذا الإجراء."
             ));

            var result = await _cardManager.GetById(CardId);
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);

        }


        [HttpPost]
        [Route("Create")]
        [ProducesDefaultResponseType(typeof(DefaultResponse<List<BusinessCard>>))]
        public async Task<IActionResult> CreateCard([FromForm] CardCreateRequest request)
        {
            var climes = HttpContext.User.Claims;
            var role = climes.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (role != "Administrator")
                return Unauthorized(DefaultResponse<bool>.FailureResponse(
                 message_en: "You do not have permission to perform this action.",
                 message_ar: "ليس لديك صلاحية لتنفيذ هذا الإجراء."
             ));

            var result = await _cardManager.Create(request);
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPut]
        [Route("update/{CardId}")]
        [ProducesDefaultResponseType(typeof(DefaultResponse<List<BusinessCard>>))]
        public async Task<IActionResult> CreateCard(int CardId, [FromForm] CardUpdateRequest request)
        {
            var climes = HttpContext.User.Claims;
            var role = climes.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (role != "Administrator")
                return Unauthorized(DefaultResponse<bool>.FailureResponse(
                 message_en: "You do not have permission to perform this action.",
                 message_ar: "ليس لديك صلاحية لتنفيذ هذا الإجراء."
             ));

            var result = await _cardManager.Update(CardId, request);

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);

        }


        [HttpPut]
        [Route("Delete/{CardId}")]
        [ProducesDefaultResponseType(typeof(DefaultResponse<List<BusinessCard>>))]
        public async Task<IActionResult> DeleteCard(int CardId)
        {
            var climes = HttpContext.User.Claims;
            var role = climes.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (role != "Administrator")
                return Unauthorized(DefaultResponse<bool>.FailureResponse(
                 message_en: "You do not have permission to perform this action.",
                 message_ar: "ليس لديك صلاحية لتنفيذ هذا الإجراء."
             ));

            var result = await _cardManager.Delete(CardId);

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);

        }

        [HttpPost]
        [Route("UploadFile")]
        [ProducesDefaultResponseType(typeof(DefaultResponse<List<BusinessCard>>))]
        public async Task<IActionResult> UploadFile(  IFormFile file)
        {
            var claims = HttpContext.User.Claims;
            var role = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (role != "Administrator")
                return Unauthorized(DefaultResponse<bool>.FailureResponse(
                    message_en: "You do not have permission to perform this action.",
                    message_ar: "ليس لديك صلاحية لتنفيذ هذا الإجراء."
                ));

            if (file == null || file.Length == 0)
                return BadRequest(DefaultResponse<bool>.FailureResponse(
                    message_en: "No file provided or the file is empty.",
                    message_ar: "لم يتم توفير ملف أو أن الملف فارغ."
                ));

            var extension = Path.GetExtension(file.FileName).ToLower();
            var allowedExtensions = new[] { ".xlsx", ".csv", ".xml" };

            if (!allowedExtensions.Contains(extension))
                return BadRequest(DefaultResponse<bool>.FailureResponse(
                    message_en: "Unsupported file type. Only XLSX, CSV, and XML are allowed.",
                    message_ar: "نوع الملف غير مدعوم. يسمح فقط بالملفات XLSX و CSV و XML."
                ));

            var result = await _cardManager.ImportFile(file);

            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result);
        }



    }
}
