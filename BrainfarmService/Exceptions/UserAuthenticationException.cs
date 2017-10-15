using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BrainfarmService.Exceptions
{
    /*
     * Exception thrown when user authentication fails due to incorrect
     * username or password
     */
    public class UserAuthenticationException : Exception
    {
        public UserAuthenticationException() : base() { }
        public UserAuthenticationException(string message) : base(message) { }
    }
}