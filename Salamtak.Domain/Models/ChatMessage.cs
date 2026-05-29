using System;

namespace Salamtak.Domain.Models
{
    public class ChatMessage : BaseEntity
    {
        public Guid PatientId { get; set; }
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Patient? Patient { get; set; }
    }
}