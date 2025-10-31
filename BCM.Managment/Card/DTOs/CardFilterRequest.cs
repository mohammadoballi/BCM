using BCM.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCM.Managment.Card.DTOs
{
    public class CardFilterRequest
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public Gender? Gender { get; set; }

        public int? PageSize { get; set; } = 10;
        public int? PageIndex { get; set; } = 1;

    }
}
