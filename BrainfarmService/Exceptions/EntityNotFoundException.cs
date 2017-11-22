using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BrainfarmService.Exceptions
{
    public class EntityNotFoundException : Exception
    {
        public Type EntityType { get; set; }
        public object EntityID { get; set; }

        public EntityNotFoundException() : base() { }
        public EntityNotFoundException(string message) : base(message) { }

        public EntityNotFoundException(Type entityType)
        {
            EntityType = entityType;
        }

        public EntityNotFoundException(Type entityType, object entityID) : this(entityType)
        {
            EntityID = entityID;
        }
    }
}