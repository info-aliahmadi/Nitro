using System;

namespace Nitro.Kernel
{
    /// <summary>
    /// Represents the base class for entities
    /// </summary>
    public abstract class BaseEntity<PKType>
    {
        public PKType Id { get; set; }

    }
}