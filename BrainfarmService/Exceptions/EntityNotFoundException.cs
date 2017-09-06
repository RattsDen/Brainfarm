using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BrainfarmService.Exceptions
{
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException() : base() { }
        public EntityNotFoundException(string message) : base(message) { }
    }
}