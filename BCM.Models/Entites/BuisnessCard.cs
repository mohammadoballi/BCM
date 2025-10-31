using BCM.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;

namespace BCM.Models.Entites
{
    public class BusinessCard
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = default!;

        [Required]
        public Gender Gender { get; set; }

        [Required]
        public DateTime BirthDate { get; set; }

        [Required]
        public string Email { get; set; } = default!;

        [Required]
        public string Phone { get; set; } = default!;

        [Required]
        public string Address { get; set; } = default!;

        public string? ImageBase64 { get; set; } = default!;
    }
}
