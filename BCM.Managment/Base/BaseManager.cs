using BCM.Models.Data;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCM.Managment.Base
{
    public class BaseManager
    {
        protected readonly IConfiguration _configuration;
        protected readonly AppDbContext _context;
        public BaseManager(IConfiguration configuration, AppDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }
    }
}
