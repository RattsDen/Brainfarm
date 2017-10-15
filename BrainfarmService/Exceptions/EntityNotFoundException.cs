using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BrainfarmService.Exceptions
{
    /*
     * Exception to be thrown when a requested entity does not exist
     */
    public class EntityNotFoundException : Exception
    {
        public EntityNotFoundException() : base() { }
        public EntityNotFoundException(string message) : base(message) { }
    }
}