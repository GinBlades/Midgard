using System;

namespace Midgard.UtilitiesN4.Models
{
    public interface IDbModelWithTimestamp
    {
        DateTime CreatedAt { get; set; }
        DateTime UpdatedAt { get; set; }
    }
}
