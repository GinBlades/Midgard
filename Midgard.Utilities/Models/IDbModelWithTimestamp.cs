using System;

namespace Midgard.Utilities.Models
{
    public interface IDbModelWithTimestamp
    {
        DateTime CreatedAt { get; set; }
        DateTime UpdatedAt { get; set; }
    }
}
