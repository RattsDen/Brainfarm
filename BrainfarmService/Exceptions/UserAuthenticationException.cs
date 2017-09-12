using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BrainfarmService.Exceptions
{
    public class UserAuthenticationException : Exception
    {
        public UserAuthenticationException() : base() { }
        public UserAuthenticationException(string message) : base(message) { }
    }
}