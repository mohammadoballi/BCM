using System;
using System.Collections.Generic;
using System.Text;

namespace BCM.Managment.Card.DTOs
{
    public class ImportXlsxFileDTO
    {
        public string name { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string gender { get; set; }
        public string address { get; set; }
        public string birthDate { get; set; }
        public string image { get; set; } 
    }

}
