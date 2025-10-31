using System;
using System.Collections.Generic;
using System.Text;

namespace BCM.Models.Enums
{
    public enum Gender
    {
        Male,
        Female
    }

    public static class GenderExtensions
    {
        public static string ToDisplayString(this Gender gender)
        {
            return gender switch
            {
                Gender.Male => "Male",
                Gender.Female => "Female",
                _ => "Unknown"
            };
        }
    }
}
