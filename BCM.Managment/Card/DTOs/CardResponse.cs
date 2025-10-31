using BCM.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BCM.Managment.Card.DTOs
{
    public class CardDetailsResponse
    {
        public int Id { get; set; }

        public string Name { get; set; } 

        public string Gender { get; set; }

        public DateTime BirthDate { get; set; }
        public string Email { get; set; } 

        public string Phone { get; set; } 

        public string Address { get; set; }

        public string? Image { get; set; } 
    }


    public class CardMinimumResponse
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Gender { get; set; }
        public string Phone { get; set; }

    }
}
