using ServiceStack.DataAnnotations;
using System;

namespace Midgard.UtilitiesN4.Models
{
    public class UserClaim
    {
        [AutoIncrement]
        public int Id { get; set; }
        public Provider Provider { get; set; }
        // JSON serialized collection of claims related to specific provider
        public string Claims { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        [ForeignKey(typeof(User), OnDelete = "CASCADE")]
        public int UserId { get; set; }
        [Reference]
        public User User { get; set; }
    }

    public enum Provider
    {
        Local, Other, Google, Facebook, Microsoft
    }
}
