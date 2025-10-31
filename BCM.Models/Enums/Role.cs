using System;
using System.Collections.Generic;
using System.Text;

namespace BCM.Models.Enums
{
    public enum Role
    {
        Admin
    }


    public static class RoleExtensions
    {
        public static string ToDisplayString(this Role role)
        {
            return role switch
            {
                Role.Admin => "Administrator",      
                _ => "Unknown"
            };
        }
    }
}
