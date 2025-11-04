using BCM.Models.Enums;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BCM.Managment.Card.DTOs
{
    public class CardCreateRequest
    {
        [Required]
        public string Name { get; set; } 
        
        [Required]
        public Gender Gender { get; set; }


        [Required]

        public DateTime BirthDate { get; set; }
        [Required]

        public string Email { get; set; } 
        [Required]

        public string Phone { get; set; } 
        [Required]

        public string Address { get; set; } 

        public IFormFile? Image { get; set; } 
    }
}
