﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainfarmService.Data
{
    public class User
    {
        // Database fields
        public int UserID { get; set; }
        public string Username { get; set; }
        public DateTime CreationDate { get; set; }
        public string Email { get; set; }
    }
}
