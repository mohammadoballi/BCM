using BCM.Models.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCM.Managment.Card.DTOs
{
    public class CardUpdateRequest
    {
        public string? Name { get; set; }
        public Gender? Gender { get; set; }

        public DateTime? BirthDate { get; set; }

        public string? Email { get; set; } 

        public string? Phone { get; set; } 

        public string? Address { get; set; } 

        public IFormFile? Image { get; set; } 
    }
}
